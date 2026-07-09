using System.Net.Http;

namespace EAEmployee.Tests.Fixtures;

/// <summary>
/// Central configuration for the integration test run. Holds the base URL
/// the tests target, the seeded admin credentials, and a one-time
/// reachability check that is used to skip the suite when the app is
/// not running.
/// </summary>
public static class AppFixture
{
    /// <summary>
    /// Where the EAEmployee app is expected to be listening. Override with the
    /// <c>EATEST_BASEURL</c> environment variable (e.g. in CI) or the launch
    /// profile of the running app.
    /// </summary>
    public static string BaseUrl { get; } =
        Environment.GetEnvironmentVariable("EATEST_BASEURL")
        ?? "http://localhost:5114";

    /// <summary>Seeded admin login from <c>SeedData.InitializeAsync</c>.</summary>
    public const string AdminUserName = "admin";
    public const string AdminPassword = "password";

    /// <summary>Default password used when the suite registers new users.</summary>
    public const string DefaultUserPassword = "Test123!";

    private static bool _availabilityChecked;
    private static bool _isAvailable;

    /// <summary>
    /// Pings the base URL once per test process. Returns false (and the
    /// Playwright tests are skipped via <see cref="Assume.That"/>) when the
    /// app is not reachable.
    /// </summary>
    public static async Task<bool> EnsureAppAvailableAsync()
    {
        if (_availabilityChecked) return _isAvailable;

        using var http = new HttpClient
        {
            // Short timeout — we just want a quick liveness signal.
            Timeout = TimeSpan.FromSeconds(2)
        };

        try
        {
            // Allow auto-redirect (HTTP → HTTPS) so the ping works on either
            // the http or https launch profile.
            http.DefaultRequestHeaders.Add("User-Agent", "EAEmployee.Tests/liveness");
            using var response = await http.GetAsync(BaseUrl);
            // 2xx, 3xx (redirect), or even 404 all count as "app is up".
            _isAvailable = (int)response.StatusCode < 500;
        }
        catch
        {
            _isAvailable = false;
        }

        _availabilityChecked = true;
        return _isAvailable;
    }

    /// <summary>Generates a unique identifier for this test process.</summary>
    public static string UniqueSuffix() => DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
}
