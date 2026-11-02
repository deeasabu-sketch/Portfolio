using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioApp.Data;
using PortfolioApp.DTOs;
using PortfolioApp.Services;

namespace PortfolioApp.Controllers;

[ApiController]
[Route("api/hire")]
public class HireController : ControllerBase
{
    private readonly EmailService _emailService;
    private readonly PortfolioDbContext _db;

    public HireController(EmailService emailService, PortfolioDbContext db)
    {
        _emailService = emailService;
        _db = db;
    }

    // POST /api/hire — public. A visitor fills this out to request a project quote.
    // The submission is emailed straight to the admin's recovery email address
    // (used here as the contact inbox too) — nothing is stored in the database.
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Submit([FromBody] HireRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.ClientName) || string.IsNullOrWhiteSpace(request.ClientEmail))
            return BadRequest(new { message = "Name and email are required." });

        if (!System.Text.RegularExpressions.Regex.IsMatch(request.ClientEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return BadRequest(new { message = "Please provide a valid email address." });

        if (string.IsNullOrWhiteSpace(request.Details))
            return BadRequest(new { message = "Please describe the project." });

        var admin = await _db.AdminUsers.FirstOrDefaultAsync();
        var notifyEmail = admin?.RecoveryEmail;
        if (string.IsNullOrWhiteSpace(notifyEmail))
            return StatusCode(500, new { message = "This site isn't set up to receive project requests yet." });

        var safeName = System.Net.WebUtility.HtmlEncode(request.ClientName);
        var safeEmail = System.Net.WebUtility.HtmlEncode(request.ClientEmail);
        var safeType = System.Net.WebUtility.HtmlEncode(request.ProjectType);
        var safeBudget = System.Net.WebUtility.HtmlEncode(request.Budget ?? "Not specified");
        var safeDetails = System.Net.WebUtility.HtmlEncode(request.Details).Replace("\n", "<br>");

        var html = $@"
            <h2>New project inquiry from your portfolio</h2>
            <p><strong>Name:</strong> {safeName}</p>
            <p><strong>Email:</strong> {safeEmail}</p>
            <p><strong>Project type:</strong> {safeType}</p>
            <p><strong>Budget:</strong> {safeBudget}</p>
            <p><strong>Wants a sample first:</strong> {(request.WantsSample ? "Yes" : "No")}</p>
            <p><strong>Details:</strong><br>{safeDetails}</p>
        ";

        try
        {
            await _emailService.SendAsync(notifyEmail, $"New project inquiry — {request.ClientName}", html);
        }
        catch (Exception)
        {
            return StatusCode(502, new { message = "Could not send the request right now. Please try again later or email directly." });
        }

        return Ok(new { message = "Thanks! Your request has been sent." });
    }
}
