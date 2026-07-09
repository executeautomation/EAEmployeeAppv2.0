using EAEmployee.Tests.Fixtures;
using FluentAssertions;
using Microsoft.Playwright;

namespace EAEmployee.Tests.Pages;

/// <summary>Page object for <c>/Account/Login</c>.</summary>
public class LoginPage
{
    private readonly IPage _page;

    public LoginPage(IPage page) => _page = page;

    public Task OpenAsync(string? returnUrl = null)
    {
        var url = AppFixture.BaseUrl.TrimEnd('/') + "/Account/Login";
        if (!string.IsNullOrEmpty(returnUrl))
        {
            url += "?returnUrl=" + Uri.EscapeDataString(returnUrl);
        }
        return _page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
    }

    public ILocator UserNameInput => _page.Locator("input[name='UserName']");
    public ILocator PasswordInput => _page.Locator("input[name='Password']");
    public ILocator RememberMeCheckbox => _page.Locator("input[name='RememberMe']");
    public ILocator SubmitButton => _page.Locator("button.btn-signin");
    public ILocator ForgotPasswordLink => _page.Locator("a[href*='ForgotPassword']");
    public ILocator CreateAccountLink => _page.Locator("a[href*='Register']");
    public ILocator ErrorSummary => _page.Locator(".alert.alert-danger");
    public ILocator HoneypotField => _page.Locator("input[name='Website']");

    /// <summary>Submits the login form and waits for the response to land.</summary>
    public async Task LoginAsync(string userName, string password, bool rememberMe = false)
    {
        await OpenAsync();
        await UserNameInput.FillAsync(userName);
        await PasswordInput.FillAsync(password);
        if (rememberMe) await RememberMeCheckbox.CheckAsync();

        // Bot-detection in the login controller requires >= 800 ms between
        // page load and submit when BotDetection:Enabled=true. Appsettings.Test
        // disables that check, but we still pace the submit so the same
        // LoginAsync() works against a non-test environment too.
        await _page.WaitForTimeoutAsync(1_000);

        await SubmitButton.ClickAsync();
        // Wait until we either navigate away or show an error.
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    }

    public async Task ShouldShowInvalidLoginErrorAsync()
    {
        await ErrorSummary.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5_000 });
        var text = await ErrorSummary.TextContentAsync();
        text.Should().Contain("Invalid login attempt",
            "the server should reject bad credentials with a visible error");
    }
}
