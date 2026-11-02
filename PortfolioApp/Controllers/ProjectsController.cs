using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioApp.Data;
using PortfolioApp.DTOs;
using PortfolioApp.Models;

namespace PortfolioApp.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly PortfolioDbContext _db;
    public ProjectsController(PortfolioDbContext db) { _db = db; }

    [HttpGet, AllowAnonymous]
    public async Task<ActionResult<List<ProjectDto>>> GetAll() =>
        Ok(await _db.Projects.OrderBy(p => p.SortOrder).ThenBy(p => p.Id)
            .Select(p => new ProjectDto
            {
                Id = p.Id, Title = p.Title, Description = p.Description, TechStack = p.TechStack,
                RepoUrl = p.RepoUrl, LiveUrl = p.LiveUrl, SortOrder = p.SortOrder
            }).ToListAsync());

    private static bool ValidOptionalUrl(string? url) =>
        string.IsNullOrWhiteSpace(url) ||
        (Uri.TryCreate(url, UriKind.Absolute, out var u) && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps));

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProjectDto>> Upsert([FromBody] ProjectDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "Title is required." });
        if (!ValidOptionalUrl(dto.RepoUrl) || !ValidOptionalUrl(dto.LiveUrl))
            return BadRequest(new { message = "Links must be full http(s):// URLs." });

        Project entity;
        if (dto.Id > 0)
        {
            var existing = await _db.Projects.FindAsync(dto.Id);
            if (existing == null) return NotFound(new { message = "Project not found." });
            entity = existing;
        }
        else
        {
            entity = new Project();
            _db.Projects.Add(entity);
        }

        entity.Title = dto.Title.Trim();
        entity.Description = dto.Description?.Trim() ?? "";
        entity.TechStack = dto.TechStack?.Trim() ?? "";
        entity.RepoUrl = string.IsNullOrWhiteSpace(dto.RepoUrl) ? null : dto.RepoUrl.Trim();
        entity.LiveUrl = string.IsNullOrWhiteSpace(dto.LiveUrl) ? null : dto.LiveUrl.Trim();
        entity.SortOrder = dto.SortOrder;
        await _db.SaveChangesAsync();

        dto.Id = entity.Id;
        return Ok(dto);
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _db.Projects.FindAsync(id);
        if (e == null) return NotFound(new { message = "Project not found." });
        _db.Projects.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
