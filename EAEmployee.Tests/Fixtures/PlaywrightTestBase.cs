using Microsoft.Playwright;
using NUnit.Framework;

namespace EAEmployee.Tests.Fixtures;

/// <summary>
/// Base class for every Playwright UI test. Owns one
/// <see cref="IBrowser"/> for the test class (launched once in
/// <see cref="OneTimeSetUp"/>) and creates an isolated
/// <see cref="IBrowserContext"/> per test in <see cref="SetUp"/> so
/// cookies, local storage, and session state never bleed between tests.
/// </summary>
public abstract class PlaywrightTestBase
{
    protected IPlaywright Playwright { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var available = await AppFixture.EnsureAppAvailableAsync();
        Assume.That(
            available,
            Is.True,
            $"EAEmployee app is not reachable at {AppFixture.BaseUrl}. " +
            "Start it with: dotnet run --project ../EAEmployee.Net8.csproj --environment Test");

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            // Speed up the suite: don't wait for the slow network idle heuristic.
            // SlowMo = 0 keeps the suite fast; we already pace form submits
            // manually when the login page's bot-detection timer needs >800 ms.
        });
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (Browser is not null) await Browser.CloseAsync();
        Playwright?.Dispose();
    }

    [SetUp]
    public virtual async Task SetUp()
    {
        Context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true, // self-signed dev cert
            ViewportSize = new ViewportSize { Width = 1366, Height = 900 },
            Locale = "en-US",
        });
        Page = await Context.NewPageAsync();
    }

    [TearDown]
    public virtual async Task TearDown()
    {
        if (Page is not null) await Page.CloseAsync();
        if (Context is not null) await Context.CloseAsync();
    }

    /// <summary>
    /// Navigates the current page to <paramref name="path"/> (relative to
    /// <see cref="AppFixture.BaseUrl"/>) and waits for the network to settle.
    /// </summary>
    protected async Task NavigateToAsync(string path)
    {
        var url = AppFixture.BaseUrl.TrimEnd('/') + "/" + path.TrimStart('/');
        await Page.GotoAsync(url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
        });
    }
}
