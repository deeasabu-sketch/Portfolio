using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioApp.Data;
using PortfolioApp.DTOs;
using PortfolioApp.Models;

namespace PortfolioApp.Controllers;

[ApiController]
[Route("api/links")]
public class LinksController : ControllerBase
{
    private readonly PortfolioDbContext _db;

    public LinksController(PortfolioDbContext db)
    {
        _db = db;
    }

    // GET /api/links — public, used by the homepage to render GitHub/LinkedIn/etc.
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<LinkDto>>> GetAll()
    {
        var links = await _db.ProfileLinks
            .OrderBy(l => l.SortOrder)
            .Select(l => new LinkDto { Id = l.Id, Label = l.Label, Url = l.Url, Type = l.Type, SortOrder = l.SortOrder })
            .ToListAsync();

        return Ok(links);
    }

    // POST /api/links — admin only. Creates a new link or updates one if Id is provided.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LinkDto>> Upsert([FromBody] UpsertLinkRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Label) || string.IsNullOrWhiteSpace(request.Url))
            return BadRequest(new { message = "Label and Url are required." });

        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
            return BadRequest(new { message = "Url must be a valid absolute URL (e.g. https://...)." });

        ProfileLink entity;
        if (request.Id is int id && id > 0)
        {
            var existing = await _db.ProfileLinks.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Link not found." });
            existing.Label = request.Label;
            existing.Url = request.Url;
            existing.Type = request.Type;
            existing.SortOrder = request.SortOrder;
            entity = existing;
        }
        else
        {
            entity = new ProfileLink
            {
                Label = request.Label,
                Url = request.Url,
                Type = request.Type,
                SortOrder = request.SortOrder
            };
            _db.ProfileLinks.Add(entity);
        }

        await _db.SaveChangesAsync();

        return Ok(new LinkDto { Id = entity.Id, Label = entity.Label, Url = entity.Url, Type = entity.Type, SortOrder = entity.SortOrder });
    }

    // DELETE /api/links/{id} — admin only.
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.ProfileLinks.FindAsync(id);
        if (existing == null) return NotFound(new { message = "Link not found." });

        _db.ProfileLinks.Remove(existing);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // PUT /api/links/reorder — admin only. Body: { "orderedIds": [3,1,2] }
    [HttpPut("reorder")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reorder([FromBody] ReorderLinksRequest request)
    {
        var links = await _db.ProfileLinks
            .Where(l => request.OrderedIds.Contains(l.Id))
            .ToListAsync();

        for (int i = 0; i < request.OrderedIds.Count; i++)
        {
            var link = links.FirstOrDefault(l => l.Id == request.OrderedIds[i]);
            if (link != null) link.SortOrder = i;
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Order updated." });
    }
}
