using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resolva.Infrastructure.Data;
using System.Text.Json;

namespace Resolva.Api.Controllers;

[ApiController]
[Route("internal/sessions")]
public class InternalSessionsController : ControllerBase
{
    private readonly ResolvaDbContext _db;
    public InternalSessionsController(ResolvaDbContext db) => _db = db;

    [HttpGet("active")]
    public async Task<IActionResult> Active([FromQuery] string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return BadRequest(new { message = "phone is required" });

        var s = await _db.SurveySessions.IgnoreQueryFilters()
            .Where(x => x.RecipientPhone == phone &&
                        (x.Status == "pending" || x.Status == "sent" || x.Status == "in_progress"))
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                sessionId = x.Id,
                tenantId = x.TenantId,
                eventId = x.EventId,
                templateId = x.TemplateId,
                status = x.Status
            })
            .FirstOrDefaultAsync();

        if (s == null) return NotFound(new { message = "No active session" });
        return Ok(s);
    }

    [HttpGet("{sessionId:guid}")]
    public async Task<IActionResult> Get(Guid sessionId)
    {
        var s = await _db.SurveySessions.IgnoreQueryFilters()
            .Where(x => x.Id == sessionId)
            .Select(x => new
            {
                sessionId = x.Id,
                tenantId = x.TenantId,
                eventId = x.EventId,
                templateId = x.TemplateId,
                recipientPhone = x.RecipientPhone,
                channel = x.Channel,
                status = x.Status,
                createdAt = x.CreatedAt,
                lastInteractionAt = x.LastInteractionAt,
                completedAt = x.CompletedAt
            })
            .FirstOrDefaultAsync();

        if (s == null) return NotFound(new { message = "Session not found" });
        return Ok(s);
    }

    [HttpGet("{sessionId:guid}/responses")]
    public async Task<IActionResult> Responses(Guid sessionId)
    {
        var rows = await _db.SurveyResponses.IgnoreQueryFilters()
            .Where(r => r.SessionId == sessionId)
            .OrderBy(r => r.QuestionId)
            .ToListAsync();

        var result = rows.Select(r => new
        {
            questionId = r.QuestionId,
            answerJson = r.AnswerJson.RootElement.Clone() 
        });

        return Ok(result);
    }
}
