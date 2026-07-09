using EAEmployee.Tests.Fixtures;
using FluentAssertions;
using Microsoft.Playwright;

namespace EAEmployee.Tests.Pages;

/// <summary>Page object for the public marketing/landing pages.</summary>
public class HomePage
{
    private readonly IPage _page;

    public HomePage(IPage page) => _page = page;

    public Task OpenAsync() => _page.GotoAsync(
        AppFixture.BaseUrl.TrimEnd('/') + "/",
        new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

    public Task OpenDashboardAsync() => _page.GotoAsync(
        AppFixture.BaseUrl.TrimEnd('/') + "/Home/Dashboard",
        new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

    public Task OpenAboutAsync() => _page.GotoAsync(
        AppFixture.BaseUrl.TrimEnd('/') + "/Home/About",
        new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

    public Task OpenContactAsync() => _page.GotoAsync(
        AppFixture.BaseUrl.TrimEnd('/') + "/Home/Contact",
        new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

    public ILocator HeroTitle =>
        _page.Locator(".ea-hero h1");

    public ILocator EmployeeCard =>
        _page.Locator(".ea-feature-card", new() { HasText = "Employee Management" });

    public ILocator LoginLink =>
        _page.Locator(".ea-navbar a", new() { HasText = "Login" });

    public ILocator RegisterLink =>
        _page.Locator(".ea-navbar a", new() { HasText = "Register" });

    public ILocator SignOutGreeting =>
        _page.Locator(".ea-navbar a[title='Manage']");

    /// <summary>
    /// Empty-state placeholder shown on the dashboard when no employees exist.
    /// </summary>
    public ILocator EmptyState =>
        _page.Locator(".ea-empty, .empty-state, [data-testid='dashboard-empty']");

    public Task ClickSignInFromHeroAsync() =>
        _page.Locator(".ea-hero a", new() { HasText = "Sign In" }).ClickAsync();

    /// <summary>Asserts the page loaded the hero section.</summary>
    public async Task ShouldBeLoadedAsync()
    {
        (await HeroTitle.IsVisibleAsync()).Should().BeTrue("the home page should render the hero title");
    }
}
