using EAEmployee.Tests.Fixtures;
using EAEmployee.Tests.Pages;
using FluentAssertions;
using NUnit.Framework;

namespace EAEmployee.Tests.Tests.Ui;

[TestFixture]
[Category("UI")]
[Parallelizable(ParallelScope.Self)]
public class AuthenticationTests : PlaywrightTestBase
{
    [Test]
    public async Task Login_As_Admin_Redirects_To_Home()
    {
        var login = new LoginPage(Page);
        await login.LoginAsync(AppFixture.AdminUserName, AppFixture.AdminPassword);

        // After successful login the form posts and we land on Home/Index.
        Page.Url.Should().NotContain("/Account/Login", "successful login should redirect away from the login page");
        var home = new HomePage(Page);
        (await home.HeroTitle.IsVisibleAsync())
            .Should().BeTrue("admin should land on the home page after signing in");
    }

    [Test]
    public async Task Login_With_Wrong_Password_Shows_Error()
    {
        var login = new LoginPage(Page);
        await login.LoginAsync(AppFixture.AdminUserName, "definitely-wrong-password");

        // Either the page still shows the error summary, or we are still on
        // /Account/Login (the form re-renders with the validation error).
        Page.Url.Should().Contain("/Account/Login",
            "a failed login should keep the user on the login page");

        await login.ShouldShowInvalidLoginErrorAsync();
    }

    [Test]
    public async Task Login_Page_Has_Forgot_Password_Link()
    {
        var login = new LoginPage(Page);
        await login.OpenAsync();

        (await login.ForgotPasswordLink.IsVisibleAsync())
            .Should().BeTrue("the login page should expose a Forgot Password link");
    }

    [Test]
    public async Task Login_Page_Has_Honeypot_Field_That_Is_Hidden()
    {
        var login = new LoginPage(Page);
        await login.OpenAsync();

        var honeypot = login.HoneypotField;
        (await honeypot.CountAsync())
            .Should().BeGreaterThan(0, "the honeypot field must be present in the DOM");

        // It should be off-screen (visually hidden) — the login page uses
        // .bot-trap { left: -9999px; top: -9999px } to keep it invisible to
        // humans but parseable by naive bots.
        var box = await honeypot.BoundingBoxAsync();
        (box == null || box.X < 0)
            .Should().BeTrue("the honeypot field should be positioned off-screen");
    }

    [Test]
    public async Task Register_New_User_Redirects_To_Home()
    {
        var suffix = AppFixture.UniqueSuffix();
        var userName = $"newuser_{suffix}";
        var email = $"{userName}@example.com";

        var register = new RegisterPage(Page);
        await register.RegisterAsync(userName, email, AppFixture.DefaultUserPassword);

        Page.Url.Should().NotContain("/Account/Register",
            "successful registration should redirect away from the register page");
        Page.Url.Should().NotContain("/Account/Login",
            "newly-registered users are signed in automatically and should land on home");
    }

    [Test]
    public async Task Register_With_Mismatched_Passwords_Shows_Error()
    {
        var register = new RegisterPage(Page);
        await register.OpenAsync();

        await register.UserNameInput.FillAsync("pwuser_" + AppFixture.UniqueSuffix());
        await register.EmailInput.FillAsync("pwuser_" + AppFixture.UniqueSuffix() + "@example.com");
        await register.PasswordInput.FillAsync(AppFixture.DefaultUserPassword);
        await register.ConfirmPasswordInput.FillAsync("DIFFERENT_" + AppFixture.DefaultUserPassword);
        await register.SubmitButton.ClickAsync();
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.DOMContentLoaded);

        Page.Url.Should().Contain("/Account/Register",
            "mismatched passwords should keep the user on the register form");
    }

    [Test]
    public async Task Register_With_Duplicate_Username_Shows_Error()
    {
        // Use the seeded admin — this should always already exist.
        var register = new RegisterPage(Page);
        await register.OpenAsync();
        await register.UserNameInput.FillAsync(AppFixture.AdminUserName);
        await register.EmailInput.FillAsync("another_" + AppFixture.UniqueSuffix() + "@example.com");
        await register.PasswordInput.FillAsync(AppFixture.DefaultUserPassword);
        await register.ConfirmPasswordInput.FillAsync(AppFixture.DefaultUserPassword);
        await register.SubmitButton.ClickAsync();
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.DOMContentLoaded);

        Page.Url.Should().Contain("/Account/Register",
            "registering an existing username should keep the user on the form");
    }

    [Test]
    public async Task Logout_After_Login_Returns_To_Anonymous_Nav()
    {
        var login = new LoginPage(Page);
        await login.LoginAsync(AppFixture.AdminUserName, AppFixture.AdminPassword);

        // Nav should now show "Hello admin!" — submit the logout form.
        var logoutForm = Page.Locator("form[action*='Logout']");
        (await logoutForm.CountAsync()).Should().BeGreaterThan(0,
            "the layout should render a logout form when the user is signed in");

        await logoutForm.Locator("button").ClickAsync();
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.DOMContentLoaded);

        // After logout the nav should expose Login / Register again.
        var home = new HomePage(Page);
        (await home.LoginLink.IsVisibleAsync())
            .Should().BeTrue("after logout the nav should show the Login link again");
    }

    [Test]
    public async Task AccessDenied_Page_Loads()
    {
        await NavigateToAsync("/Account/AccessDenied");
        var heading = Page.Locator("h1, h2, h3").First;
        (await heading.IsVisibleAsync()).Should().BeTrue();
    }
}
