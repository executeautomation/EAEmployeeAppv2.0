using EAEmployee.Tests.Fixtures;
using Microsoft.Playwright;

namespace EAEmployee.Tests.Pages;

/// <summary>Page object for <c>/Account/Register</c>.</summary>
public class RegisterPage
{
    private readonly IPage _page;

    public RegisterPage(IPage page) => _page = page;

    public Task OpenAsync() => _page.GotoAsync(
        AppFixture.BaseUrl.TrimEnd('/') + "/Account/Register",
        new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

    public ILocator UserNameInput => _page.Locator("input[name='UserName']");
    public ILocator EmailInput => _page.Locator("input[name='Email']");
    public ILocator PasswordInput => _page.Locator("input[name='Password']");
    public ILocator ConfirmPasswordInput => _page.Locator("input[name='ConfirmPassword']");
    public ILocator SubmitButton => _page.Locator("button.btn-register");
    public ILocator LoginLink => _page.Locator("a[href*='Login']");
    public ILocator ValidationSummary => _page.Locator("#val-summary");

    public async Task RegisterAsync(string userName, string email, string password)
    {
        await OpenAsync();
        await UserNameInput.FillAsync(userName);
        await EmailInput.FillAsync(email);
        await PasswordInput.FillAsync(password);
        await ConfirmPasswordInput.FillAsync(password);
        await SubmitButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    }
}
