using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioApp.Data;
using PortfolioApp.DTOs;
using PortfolioApp.Models;

namespace PortfolioApp.Controllers;

[ApiController]
[Route("api/skills")]
public class SkillsController : ControllerBase
{
    private readonly PortfolioDbContext _db;
    public SkillsController(PortfolioDbContext db) { _db = db; }

    [HttpGet, AllowAnonymous]
    public async Task<ActionResult<List<SkillDto>>> GetAll() =>
        Ok(await _db.Skills.OrderBy(s => s.SortOrder).ThenBy(s => s.Id)
            .Select(s => new SkillDto { Id = s.Id, Title = s.Title, Description = s.Description, Details = s.Details, Level = s.Level, SortOrder = s.SortOrder })
            .ToListAsync());

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<ActionResult<SkillDto>> Upsert([FromBody] SkillDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "Title is required." });

        Skill entity;
        if (dto.Id > 0)
        {
            var existing = await _db.Skills.FindAsync(dto.Id);
            if (existing == null) return NotFound(new { message = "Skill not found." });
            entity = existing;
        }
        else
        {
            entity = new Skill();
            _db.Skills.Add(entity);
        }

        entity.Title = dto.Title.Trim();
        entity.Description = dto.Description?.Trim() ?? "";
        entity.Details = dto.Details?.Trim() ?? "";
        entity.Level = Math.Clamp(dto.Level, 0, 100);
        entity.SortOrder = dto.SortOrder;
        await _db.SaveChangesAsync();

        dto.Id = entity.Id;
        return Ok(dto);
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _db.Skills.FindAsync(id);
        if (e == null) return NotFound(new { message = "Skill not found." });
        _db.Skills.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
