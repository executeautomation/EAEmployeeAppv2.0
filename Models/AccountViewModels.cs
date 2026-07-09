using System.ComponentModel.DataAnnotations;

namespace EAEmployee.Net8.Models;

public class LoginViewModel
{
    [Required]
    [Display(Name = "User Name")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }

    // ── Bot-detection fields ──────────────────────────────────────────────────
    // Honeypot: human visitors never see or fill this field (hidden via CSS).
    public string? Website { get; set; }

    // JavaScript challenge token: set to "ok" by the login page script.
    public string? CaptchaToken { get; set; }

    // Unix timestamp (ms) at which the page finished loading, written by JS.
    public string? PageLoadTime { get; set; }
}

public class RegisterViewModel
{
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [Display(Name = "User Name")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}


