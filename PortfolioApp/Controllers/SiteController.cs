using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioApp.Data;
using PortfolioApp.DTOs;
using PortfolioApp.Models;

namespace PortfolioApp.Controllers;

[ApiController]
[Route("api/site")]
public class SiteController : ControllerBase
{
    private readonly PortfolioDbContext _db;
    private readonly IWebHostEnvironment _env;
    private const long MaxPhotoBytes = 5 * 1024 * 1024; // 5 MB

    public SiteController(PortfolioDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    private async Task<SiteProfile> GetOrCreateProfile()
    {
        var p = await _db.SiteProfiles.FirstOrDefaultAsync();
        if (p == null) { p = new SiteProfile(); _db.SiteProfiles.Add(p); await _db.SaveChangesAsync(); }
        return p;
    }

    // ---------- Editable homepage text ----------
    [HttpGet("text"), AllowAnonymous]
    public async Task<ActionResult<SiteTextDto>> GetText()
    {
        var p = await GetOrCreateProfile();
        return Ok(new SiteTextDto { HeroTitle = p.HeroTitle, HeroText = p.HeroText, AboutText = p.AboutText });
    }

    [HttpPut("text"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateText([FromBody] SiteTextDto dto)
    {
        var p = await GetOrCreateProfile();
        p.HeroTitle = string.IsNullOrWhiteSpace(dto.HeroTitle) ? null : dto.HeroTitle.Trim();
        p.HeroText = string.IsNullOrWhiteSpace(dto.HeroText) ? null : dto.HeroText.Trim();
        p.AboutText = string.IsNullOrWhiteSpace(dto.AboutText) ? null : dto.AboutText.Trim();
        await _db.SaveChangesAsync();
        return Ok(new { message = "Text updated." });
    }

    // ---------- Profile photo ----------
    [HttpGet("photo"), AllowAnonymous]
    public async Task<ActionResult<PhotoInfoDto>> GetPhoto()
    {
        var p = await GetOrCreateProfile();
        if (string.IsNullOrEmpty(p.PhotoStoredPath))
            return Ok(new PhotoInfoDto { Url = "assets/profile.jpg", IsCustom = false });

        // ?v= makes the browser reload the image right after you replace it
        var v = p.PhotoUpdatedAt?.Ticks ?? 0;
        return Ok(new PhotoInfoDto { Url = $"{p.PhotoStoredPath.Replace('\\', '/')}?v={v}", IsCustom = true });
    }

    [HttpPost("photo"), Authorize(Roles = "Admin")]
    [RequestSizeLimit(MaxPhotoBytes)]
    public async Task<ActionResult<PhotoInfoDto>> UploadPhoto(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file was uploaded." });
        if (file.Length > MaxPhotoBytes)
            return BadRequest(new { message = "Photo is too large. Max size is 5 MB." });

        // Check the real file signature, not just the file name.
        var header = new byte[12];
        using (var s = file.OpenReadStream()) { await s.ReadAsync(header, 0, header.Length); }

        string? ext = null;
        if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) ext = ".jpg";
        else if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) ext = ".png";
        else if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                 header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50) ext = ".webp";

        if (ext == null)
            return BadRequest(new { message = "Only JPG, PNG or WebP images are accepted." });

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        // Remove any previous photo (may have a different extension)
        foreach (var old in Directory.GetFiles(uploadsDir, "profile-photo.*"))
            System.IO.File.Delete(old);

        var storedName = "profile-photo" + ext;
        using (var stream = new FileStream(Path.Combine(uploadsDir, storedName), FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var p = await GetOrCreateProfile();
        p.PhotoStoredPath = "uploads/" + storedName;
        p.PhotoUpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new PhotoInfoDto { Url = $"{p.PhotoStoredPath}?v={p.PhotoUpdatedAt.Value.Ticks}", IsCustom = true });
    }

    [HttpDelete("photo"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetPhoto()
    {
        var p = await GetOrCreateProfile();
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        if (Directory.Exists(uploadsDir))
            foreach (var old in Directory.GetFiles(uploadsDir, "profile-photo.*"))
                System.IO.File.Delete(old);
        p.PhotoStoredPath = null;
        p.PhotoUpdatedAt = null;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Reverted to the default photo." });
    }
}
