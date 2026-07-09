using EAEmployee.Tests.Pages;
using Microsoft.Playwright;

namespace EAEmployee.Tests.Fixtures;

/// <summary>
/// Convenience helpers for UI tests that need a signed-in session.
/// Each test gets a fresh <see cref="IBrowserContext"/>, so signing in
/// inside the test (rather than once per class) keeps state isolated.
/// </summary>
public abstract class AuthenticatedPageBase : PlaywrightTestBase
{
    /// <summary>Signs the current page in as the seeded admin user.</summary>
    protected async Task LoginAsAdminAsync()
    {
        var login = new LoginPage(Page);
        await login.LoginAsync(AppFixture.AdminUserName, AppFixture.AdminPassword);
    }

    /// <summary>Signs in as a freshly-registered standard (non-admin) user.</summary>
    protected async Task<(string userName, string email)> RegisterFreshUserAsync()
    {
        var suffix = AppFixture.UniqueSuffix();
        var userName = $"testuser_{suffix}";
        var email = $"{userName}@example.com";
        var password = AppFixture.DefaultUserPassword;

        var register = new RegisterPage(Page);
        await register.RegisterAsync(userName, email, password);

        return (userName, email);
    }
}
