namespace PortfolioApp.DTOs;

public class LoginRequest
{
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    // No fields needed — there is only one admin account, and the recovery
    // email address is already stored server-side. Kept as a class for
    // forward-compatibility (e.g. adding a captcha token later).
}

public class ResetPasswordRequest
{
    public string Code { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class UpdateRecoveryEmailRequest
{
    public string Email { get; set; } = string.Empty;
}
