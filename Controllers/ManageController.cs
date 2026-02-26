using EAEmployee.Net8.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EAEmployee.Net8.Controllers;

[Authorize]
public class ManageController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public ManageController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // GET: /Manage/Index
    public async Task<IActionResult> Index(ManageMessageId? message)
    {
        ViewBag.StatusMessage = message switch
        {
            ManageMessageId.ChangePasswordSuccess => "Your password has been changed.",
            ManageMessageId.SetPasswordSuccess => "Your password has been set.",
            ManageMessageId.Error => "An error has occurred.",
            _ => string.Empty
        };

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var model = new IndexViewModel
        {
            UserName = user.UserName,
            Email = user.Email,
            HasPassword = await _userManager.HasPasswordAsync(user)
        };
        return View(model);
    }

    // GET: /Manage/ChangePassword
    public IActionResult ChangePassword() => View();

    // POST: /Manage/ChangePassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.ChangePasswordSuccess });
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    // GET: /Manage/SetPassword
    public IActionResult SetPassword() => View();

    // POST: /Manage/SetPassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.SetPasswordSuccess });
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }
}
