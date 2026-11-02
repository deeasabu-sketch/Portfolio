namespace PortfolioApp.Models;

// Single-row table: there is only ever one admin (you).
// No username is stored on purpose — the login form only asks for a password.
public class AdminUser
{
    public int Id { get; set; }
    public string PasswordHash { get; set; } = string.Empty;

    // Where the "forgot password" one-time code is sent. Not a login username —
    // login is still password-only. Set from configuration on first run.
    public string RecoveryEmail { get; set; } = string.Empty;
}
