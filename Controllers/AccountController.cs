using EAEmployee.Net8.Models;
using EAEmployee.Net8.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EAEmployee.Net8.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly BotDetectionService _botDetection;
    private readonly CaptchaService _captcha;
    private readonly IConfiguration _configuration;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        BotDetectionService botDetection,
        CaptchaService captcha,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _botDetection = botDetection;
        _captcha = captcha;
        _configuration = configuration;
    }

    // GET: /Account/Login
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        ViewBag.RecaptchaSiteKey = _configuration["GoogleRecaptcha:SiteKey"];
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        ViewBag.RecaptchaSiteKey = _configuration["GoogleRecaptcha:SiteKey"];

        if (!ModelState.IsValid) return View(model);

        // ── Google reCAPTCHA v2 validation ────────────────────────────────────
        var recaptchaToken = Request.Form["g-recaptcha-response"].ToString();
        if (!await _captcha.ValidateAsync(recaptchaToken))
        {
            ModelState.AddModelError(string.Empty, "Please complete the CAPTCHA verification.");
            return View(model);
        }

        // ── Bot / automated-client detection ─────────────────────────────────
        var botResult = _botDetection.Analyze(
            HttpContext,
            model.Website,
            model.CaptchaToken,
            model.PageLoadTime);

        if (botResult.IsBot)
        {
            ModelState.AddModelError(string.Empty, botResult.Reason);
            return View(model);
        }
        // ─────────────────────────────────────────────────────────────────────

        var result = await _signInManager.PasswordSignInAsync(
            model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
            return RedirectToLocal(returnUrl);

        if (result.IsLockedOut)
            return View("Lockout");

        _botDetection.RecordFailedAttempt(HttpContext);

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    // GET: /Account/Register
    [AllowAnonymous]
    public IActionResult Register() => View();

    // POST: /Account/Register
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "User");
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/AccessDenied
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    // GET: /Account/ForgotPassword
    [AllowAnonymous]
    public IActionResult ForgotPassword() => View();

    // POST: /Account/ForgotPassword
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }
        return View(model);
    }

    // GET: /Account/ForgotPasswordConfirmation
    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation() => View();

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }
}
