using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioApp.Data;
using PortfolioApp.DTOs;
using PortfolioApp.Models;

namespace PortfolioApp.Controllers;

[ApiController]
[Route("api")]
public class ProfileController : ControllerBase
{
    private readonly PortfolioDbContext _db;
    private readonly IWebHostEnvironment _env;
    private const long MaxCvSizeBytes = 10 * 1024 * 1024; // 10 MB

    public ProfileController(PortfolioDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // GET /api/cv — public. Tells the frontend whether a CV exists and where to download it.
    [HttpGet("cv")]
    [AllowAnonymous]
    public async Task<ActionResult<CvInfoDto>> GetCvInfo()
    {
        var profile = await _db.SiteProfiles.FirstOrDefaultAsync();
        if (profile == null || string.IsNullOrEmpty(profile.CvStoredPath))
        {
            return Ok(new CvInfoDto { HasCv = false });
        }

        return Ok(new CvInfoDto
        {
            HasCv = true,
            FileName = profile.CvFileName,
            UpdatedAtUtc = profile.CvUpdatedAt,
            DownloadUrl = "/api/cv/download"
        });
    }

    // GET /api/cv/download — public. Streams the current CV file to any visitor.
    [HttpGet("cv/download")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadCv()
    {
        var profile = await _db.SiteProfiles.FirstOrDefaultAsync();
        if (profile == null || string.IsNullOrEmpty(profile.CvStoredPath))
            return NotFound(new { message = "No CV has been uploaded yet." });

        var fullPath = Path.Combine(_env.WebRootPath, profile.CvStoredPath);
        if (!System.IO.File.Exists(fullPath))
            return NotFound(new { message = "CV file is missing on the server." });

        var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
        var downloadName = string.IsNullOrWhiteSpace(profile.CvFileName) ? "CV.pdf" : profile.CvFileName;
        return File(bytes, "application/pdf", downloadName);
    }

    // POST /api/cv — admin only. Upload a new CV (PDF), replacing the previous one.
    [HttpPost("cv")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(MaxCvSizeBytes)]
    public async Task<ActionResult<CvInfoDto>> UploadCv(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file was uploaded." });

        if (file.Length > MaxCvSizeBytes)
            return BadRequest(new { message = "File is too large. Max size is 10 MB." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".pdf")
            return BadRequest(new { message = "Only PDF files are accepted for the CV." });

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        // Fixed, predictable file name on disk — avoids accumulating old files
        // and avoids trusting the client-supplied name for the physical path.
        var storedFileName = "cv-current.pdf";
        var storedRelativePath = Path.Combine("uploads", storedFileName);
        var fullPath = Path.Combine(uploadsDir, storedFileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var profile = await _db.SiteProfiles.FirstOrDefaultAsync();
        if (profile == null)
        {
            profile = new SiteProfile();
            _db.SiteProfiles.Add(profile);
        }

        profile.CvFileName = string.IsNullOrWhiteSpace(file.FileName) ? "CV.pdf" : file.FileName;
        profile.CvStoredPath = storedRelativePath;
        profile.CvUpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new CvInfoDto
        {
            HasCv = true,
            FileName = profile.CvFileName,
            UpdatedAtUtc = profile.CvUpdatedAt,
            DownloadUrl = "/api/cv/download"
        });
    }
}
