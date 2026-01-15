using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resolva.Api.Contracts.Internal.Surveys;
using Resolva.Core.Entities;
using Resolva.Infrastructure.Data;
using System.Text.Json;

namespace Resolva.Api.Controllers;

// IMPORTANT: In production, protect /internal with HMAC middleware (via Express Gateway).
// For now we keep it simple; we'll add HMAC in the next step.
[ApiController]
[Route("internal/surveys")]
public class InternalSurveysController : ControllerBase
{
    private readonly ResolvaDbContext _db;
    public InternalSurveysController(ResolvaDbContext db) => _db = db;

    // n8n: AI generated schema -> save template + create session
    [HttpPost("generated")]
    public async Task<ActionResult<GenerateSurveyResponse>> Generated([FromBody] GenerateSurveyRequest req)
    {
        // Ensure event exists (and belongs to tenant via IgnoreQueryFilters + check)
        var ev = await _db.Events.IgnoreQueryFilters()
            .SingleOrDefaultAsync(e => e.Id == req.EventId && e.TenantId == req.TenantId);

        if (ev == null) return NotFound(new { message = "Event not found" });

        // Create template
        var template = new SurveyTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = req.TenantId,
            EventType = ev.EventType,
            Language = string.IsNullOrWhiteSpace(req.Language) ? "en" : req.Language.Trim(),
            Version = 1,
            CreatedBy = string.IsNullOrWhiteSpace(req.CreatedBy) ? "ai" : req.CreatedBy.Trim(),
            SchemaJson = JsonDocument.Parse(req.SchemaJson.GetRawText())
        };

        _db.SurveyTemplates.Add(template);

        // Create session (MVP: one per event)
        var session = new SurveySession
        {
            Id = Guid.NewGuid(),
            TenantId = req.TenantId,
            EventId = ev.Id,
            TemplateId = template.Id,
            RecipientPhone = ev.ContactPhone,
            Channel = "whatsapp",
            Status = "pending"
        };

        _db.SurveySessions.Add(session);

        await _db.SaveChangesAsync();

        return Ok(new GenerateSurveyResponse(template.Id, session.Id));
    }

    // n8n: save answer
    [HttpPost("answer")]
    public async Task<IActionResult> Answer([FromBody] SurveyAnswerRequest req)
    {
        var session = await _db.SurveySessions.IgnoreQueryFilters()
            .SingleOrDefaultAsync(s => s.Id == req.SessionId && s.TenantId == req.TenantId);

        if (session == null) return NotFound(new { message = "Session not found" });

        // Upsert per question (MVP)
        var existing = await _db.SurveyResponses.IgnoreQueryFilters()
            .SingleOrDefaultAsync(r => r.SessionId == req.SessionId && r.TenantId == req.TenantId && r.QuestionId == req.QuestionId);

        if (existing == null)
        {
            existing = new SurveyResponse
            {
                Id = Guid.NewGuid(),
                TenantId = req.TenantId,
                SessionId = req.SessionId,
                QuestionId = req.QuestionId,
                AnswerJson = JsonDocument.Parse(req.AnswerJson.GetRawText())
            };
            _db.SurveyResponses.Add(existing);
        }
        else
        {
            existing.AnswerJson = JsonDocument.Parse(req.AnswerJson.GetRawText());
        }

        session.LastInteractionAt = req.AnsweredAt ?? DateTimeOffset.UtcNow;
        if (session.Status == "pending") session.Status = "in_progress";

        await _db.SaveChangesAsync();
        return Ok(new { message = "saved" });
    }

    // n8n: mark completed + compute outcome (simple MVP rules)
    [HttpPost("complete")]
    public async Task<IActionResult> Complete([FromBody] CompleteSurveyRequest req)
    {
        var session = await _db.SurveySessions.IgnoreQueryFilters()
            .SingleOrDefaultAsync(s => s.Id == req.SessionId && s.TenantId == req.TenantId);

        if (session == null) return NotFound(new { message = "Session not found" });

        session.Status = "completed";
        session.CompletedAt = DateTimeOffset.UtcNow;

        // Simple MVP outcome computation:
        // - confirmation = confirmed if answer to questionId "q1" is yes OR "resolved" is true
        // - satisfaction = rating from questionId "rating" if present
        var responses = await _db.SurveyResponses.IgnoreQueryFilters()
            .Where(r => r.SessionId == req.SessionId && r.TenantId == req.TenantId)
            .ToListAsync();

        string confirmation = "partial";
        decimal? rating = null;

        foreach (var r in responses)
        {
            if (r.QuestionId == "q1")
            {
                // expecting answerJson like { "value": "yes" } or { "value": true }
                if (r.AnswerJson.RootElement.TryGetProperty("value", out var v))
                {
                    if (v.ValueKind == JsonValueKind.String && v.GetString()?.ToLower() == "yes") confirmation = "confirmed";
                    if (v.ValueKind == JsonValueKind.True) confirmation = "confirmed";
                    if (v.ValueKind == JsonValueKind.False) confirmation = "not_confirmed";
                    if (v.ValueKind == JsonValueKind.String && v.GetString()?.ToLower() == "no") confirmation = "not_confirmed";
                }
            }

            if (r.QuestionId == "rating" && r.AnswerJson.RootElement.TryGetProperty("value", out var rv))
            {
                if (rv.ValueKind == JsonValueKind.Number && rv.TryGetDecimal(out var d)) rating = d;
                if (rv.ValueKind == JsonValueKind.String && decimal.TryParse(rv.GetString(), out var ds)) rating = ds;
            }
        }

        // Upsert outcome
        var outcome = await _db.SurveyOutcomes.IgnoreQueryFilters()
            .SingleOrDefaultAsync(o => o.SessionId == req.SessionId && o.TenantId == req.TenantId);

        if (outcome == null)
        {
            outcome = new SurveyOutcome
            {
                SessionId = req.SessionId,
                TenantId = req.TenantId
            };
            _db.SurveyOutcomes.Add(outcome);
        }

        outcome.ConfirmationStatus = confirmation;
        outcome.SatisfactionScore = rating;
        outcome.ComputedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { message = "completed", confirmation, rating });
    }
}
