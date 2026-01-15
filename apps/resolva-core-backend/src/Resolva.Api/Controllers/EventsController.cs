using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resolva.Api.Contracts.Events;
using Resolva.Infrastructure.Data;
using System.Text.Json;

namespace Resolva.Api.Controllers;

[ApiController]
[Route("api/events")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly ResolvaDbContext _db;
    public EventsController(ResolvaDbContext db) => _db = db;

    [HttpPost]
    public async Task<ActionResult<EventResponse>> Create([FromBody] CreateEventRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.EventType))
            return BadRequest(new { message = "eventType is required" });

        if (string.IsNullOrWhiteSpace(req.ContactPhone))
            return BadRequest(new { message = "contactPhone is required" });

        if (req.ProductId.HasValue && req.ServiceId.HasValue)
            return BadRequest(new { message = "Use productId OR serviceId, not both (MVP rule)" });

        var metadataDoc = req.Metadata.HasValue
            ? JsonDocument.Parse(req.Metadata.Value.GetRawText())
            : JsonDocument.Parse("{}");

        var ev = new Core.Entities.Event
        {
            Id = Guid.NewGuid(),
            EventType = req.EventType.Trim(),
            CustomerId = req.CustomerId,
            ContactPhone = req.ContactPhone.Trim(),
            ProductId = req.ProductId,
            ServiceId = req.ServiceId,
            Status = "created",
            Metadata = metadataDoc,
            OccurredAt = req.OccurredAt
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        return Ok(ToResponse(ev));
    }

    [HttpGet]
    public async Task<ActionResult<List<EventResponse>>> List(
        [FromQuery] string? status = null,
        [FromQuery] string? eventType = null,
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? serviceId = null)
    {
        var q = _db.Events.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status);
        if (!string.IsNullOrWhiteSpace(eventType)) q = q.Where(x => x.EventType == eventType);
        if (productId.HasValue) q = q.Where(x => x.ProductId == productId);
        if (serviceId.HasValue) q = q.Where(x => x.ServiceId == serviceId);

        var items = await q.OrderByDescending(x => x.OccurredAt).Take(200).ToListAsync();
        return Ok(items.Select(ToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EventResponse>> Get(Guid id)
    {
        var ev = await _db.Events.SingleOrDefaultAsync(x => x.Id == id);
        if (ev == null) return NotFound();
        return Ok(ToResponse(ev));
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<EventResponse>> Update(Guid id, [FromBody] UpdateEventRequest req)
    {
        var ev = await _db.Events.SingleOrDefaultAsync(x => x.Id == id);
        if (ev == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(req.Status))
            ev.Status = req.Status.Trim();

        if (req.Metadata.HasValue)
            ev.Metadata = JsonDocument.Parse(req.Metadata.Value.GetRawText());

        await _db.SaveChangesAsync();
        return Ok(ToResponse(ev));
    }

    private static EventResponse ToResponse(Core.Entities.Event ev)
    {
        var json = JsonDocument.Parse(ev.Metadata.RootElement.GetRawText()).RootElement;
        return new EventResponse(
            ev.Id,
            ev.EventType,
            ev.CustomerId,
            ev.ContactPhone,
            ev.ProductId,
            ev.ServiceId,
            ev.Status,
            json,
            ev.OccurredAt,
            ev.CreatedAt
        );
    }
}
