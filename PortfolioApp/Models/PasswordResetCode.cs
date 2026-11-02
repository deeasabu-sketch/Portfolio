namespace PortfolioApp.Models;

// A short-lived one-time code emailed to the admin's recovery address
// to allow a password reset without knowing the current password.
public class PasswordResetCode
{
    public int Id { get; set; }
    public string CodeHash { get; set; } = string.Empty; // the OTP itself, hashed — never stored in plain text
    public DateTime ExpiresAtUtc { get; set; }
    public bool Used { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
