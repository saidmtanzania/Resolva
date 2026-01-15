using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resolva.Api.Contracts.Catalog;
using Resolva.Core.Entities;
using Resolva.Infrastructure.Data;

namespace Resolva.Api.Controllers;

[ApiController]
[Route("api/products")]
[Authorize] // any logged-in agent for MVP
public class ProductsController : ControllerBase
{
    private readonly ResolvaDbContext _db;
    public ProductsController(ResolvaDbContext db) => _db = db;

    [HttpPost]
    public async Task<ActionResult<CatalogItemResponse>> Create(CreateCatalogItemRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest(new { message = "Name is required" });

        var exists = await _db.Products.AnyAsync(p => p.Name.ToLower() == req.Name.Trim().ToLower());
        if (exists) return Conflict(new { message = "Product name already exists" });

        var item = new Product
        {
            Id = Guid.NewGuid(),
            Name = req.Name.Trim(),
            Category = string.IsNullOrWhiteSpace(req.Category) ? null : req.Category.Trim(),
            IsActive = true
        };

        _db.Products.Add(item);
        await _db.SaveChangesAsync();

        return Ok(new CatalogItemResponse(item.Id, item.Name, item.Category, item.IsActive, item.CreatedAt));
    }

    [HttpGet]
    public async Task<ActionResult<List<CatalogItemResponse>>> List([FromQuery] bool? active = null, [FromQuery] string? search = null)
    {
        var q = _db.Products.AsQueryable();

        if (active.HasValue) q = q.Where(x => x.IsActive == active.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(x => x.Name.ToLower().Contains(s) || (x.Category != null && x.Category.ToLower().Contains(s)));
        }

        var items = await q.OrderByDescending(x => x.CreatedAt).ToListAsync();
        return Ok(items.Select(x => new CatalogItemResponse(x.Id, x.Name, x.Category, x.IsActive, x.CreatedAt)).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CatalogItemResponse>> Get(Guid id)
    {
        var item = await _db.Products.SingleOrDefaultAsync(x => x.Id == id);
        if (item == null) return NotFound();

        return Ok(new CatalogItemResponse(item.Id, item.Name, item.Category, item.IsActive, item.CreatedAt));
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<CatalogItemResponse>> Update(Guid id, UpdateCatalogItemRequest req)
    {
        var item = await _db.Products.SingleOrDefaultAsync(x => x.Id == id);
        if (item == null) return NotFound();

        if (req.Name != null && string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { message = "Name cannot be empty" });

        if (!string.IsNullOrWhiteSpace(req.Name))
            item.Name = req.Name.Trim();

        if (req.Category != null)
            item.Category = string.IsNullOrWhiteSpace(req.Category) ? null : req.Category.Trim();

        if (req.IsActive.HasValue)
            item.IsActive = req.IsActive.Value;

        await _db.SaveChangesAsync();

        return Ok(new CatalogItemResponse(item.Id, item.Name, item.Category, item.IsActive, item.CreatedAt));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var item = await _db.Products.SingleOrDefaultAsync(x => x.Id == id);
        if (item == null) return NotFound();

        item.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
