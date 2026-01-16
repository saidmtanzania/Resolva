using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resolva.Api.Contracts.SurveyTemplates;
using Resolva.Api.Services;
using Resolva.Core.Entities;
using Resolva.Infrastructure.Data;
using System.Text.Json;

namespace Resolva.Api.Controllers;

[ApiController]
[Route("api/survey-templates")]
[Authorize] // later: AdminOnly
public class SurveyTemplatesController : ControllerBase
{
    private readonly ResolvaDbContext _db;
    private readonly GatewayClient _gateway;

    public SurveyTemplatesController(ResolvaDbContext db, GatewayClient gateway)
    {
        _db = db;
        _gateway = gateway;
    }

    [HttpPost]
    public async Task<ActionResult<SurveyTemplateResponse>> Create(CreateSurveyTemplateRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest(new { message = "name required" });
        if (string.IsNullOrWhiteSpace(req.EventType)) return BadRequest(new { message = "eventType required" });

        var template = new SurveyTemplate
        {
            Id = Guid.NewGuid(),
            Name = req.Name.Trim(),
            EventType = req.EventType.Trim(),
            Language = string.IsNullOrWhiteSpace(req.Language) ? "en" : req.Language.Trim(),
            Version = 1,
            IsActive = req.IsActive,
            Channel = "whatsapp_flow",
            SchemaJson = JsonDocument.Parse(req.FlowJson.GetRawText()),
            CreatedBy = User.Identity?.Name ?? "dashboard",
            WhatsAppStatus = "DRAFT",
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // If activating this template, deactivate others for same eventType+language
        if (template.IsActive)
        {
            await _db.SurveyTemplates
                .Where(t => t.EventType == template.EventType && t.Language == template.Language && t.IsActive)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false)
                .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow));
        }

        _db.SurveyTemplates.Add(template);
        await _db.SaveChangesAsync();

        return Ok(ToResponse(template));
    }

    [HttpGet]
    public async Task<ActionResult<List<SurveyTemplateResponse>>> List([FromQuery] string? eventType = null, [FromQuery] string? language = null)
    {
        var q = _db.SurveyTemplates.AsQueryable();
        if (!string.IsNullOrWhiteSpace(eventType)) q = q.Where(x => x.EventType == eventType);
        if (!string.IsNullOrWhiteSpace(language)) q = q.Where(x => x.Language == language);

        var items = await q.OrderByDescending(x => x.UpdatedAt).ToListAsync();
        return Ok(items.Select(ToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SurveyTemplateResponse>> Get(Guid id)
    {
        var t = await _db.SurveyTemplates.SingleOrDefaultAsync(x => x.Id == id);
        if (t == null) return NotFound();
        return Ok(ToResponse(t));
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<SurveyTemplateResponse>> Update(Guid id, UpdateSurveyTemplateRequest req)
    {
        var t = await _db.SurveyTemplates.SingleOrDefaultAsync(x => x.Id == id);
        if (t == null) return NotFound();

        // Don’t allow editing once published (clone later)
        if (t.WhatsAppStatus == "PUBLISHED")
            return BadRequest(new { message = "Template already published. Clone to make changes." });

        if (req.Name != null) t.Name = req.Name.Trim();
        if (req.FlowJson.HasValue) t.SchemaJson = JsonDocument.Parse(req.FlowJson.Value.GetRawText());
        if (req.IsActive.HasValue)
        {
            t.IsActive = req.IsActive.Value;

            if (t.IsActive)
            {
                await _db.SurveyTemplates
                    .Where(x => x.Id != t.Id && x.EventType == t.EventType && x.Language == t.Language && x.IsActive)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false)
                    .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow));
            }
        }

        t.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ToResponse(t));
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id)
    {
        var t = await _db.SurveyTemplates.SingleOrDefaultAsync(x => x.Id == id);
        if (t == null) return NotFound();

        if (t.WhatsAppStatus == "PUBLISHED")
            return BadRequest(new { message = "Already published" });

        try
        {
            // 1) Create flow if not created
            if (string.IsNullOrWhiteSpace(t.WhatsAppFlowId))
            {
                var created = await _gateway.CreateFlowAsync(t.Name, new[] { "SURVEY" });
                t.WhatsAppFlowId = created.FlowId;
                t.WhatsAppStatus = "DRAFT";
                t.UpdatedAt = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync();
            }

            // 2) Upload JSON (validation happens here)
            var upload = await _gateway.UploadFlowJsonAsync(t.WhatsAppFlowId!, t.SchemaJson.RootElement);

            if (!upload.Ok)
            {
                t.WhatsAppStatus = "ERROR";
                t.ValidationErrors = upload.Errors.HasValue
                    ? JsonDocument.Parse(upload.Errors.Value.GetRawText())
                    : JsonDocument.Parse("{\"message\":\"Upload failed\"}");
                t.UpdatedAt = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync();

                return BadRequest(new { message = "Flow JSON validation failed", errors = upload.Errors });
            }

            t.ValidationErrors = null;
            t.WhatsAppStatus = "DRAFT";
            t.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();

            // 3) Publish
            await _gateway.PublishFlowAsync(t.WhatsAppFlowId!);

            t.WhatsAppStatus = "PUBLISHED";
            t.PublishedAt = DateTimeOffset.UtcNow;
            t.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Published", flowId = t.WhatsAppFlowId, publishedAt = t.PublishedAt });
        }
        catch (Exception ex)
        {
            t.WhatsAppStatus = "ERROR";
            t.ValidationErrors = JsonDocument.Parse(JsonSerializer.Serialize(new { error = ex.Message }));
            t.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
            return StatusCode(500, new { message = "Publish failed", error = ex.Message });
        }
    }

    private static SurveyTemplateResponse ToResponse(SurveyTemplate t)
    {
        JsonElement? errors = null;
        if (t.ValidationErrors != null)
            errors = JsonDocument.Parse(t.ValidationErrors.RootElement.GetRawText()).RootElement;

        var flowJson = JsonDocument.Parse(t.SchemaJson.RootElement.GetRawText()).RootElement;

        return new SurveyTemplateResponse(
            t.Id, t.Name, t.EventType, t.Language, t.Version, t.IsActive, t.Channel,
            t.WhatsAppFlowId, t.WhatsAppStatus, t.PublishedAt, errors,
            t.CreatedAt, t.UpdatedAt, flowJson
        );
    }
}
