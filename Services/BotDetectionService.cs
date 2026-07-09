using Microsoft.Extensions.Caching.Memory;

namespace EAEmployee.Net8.Services;

/// <summary>
/// Signals indicating why traffic was classified as automated.
/// </summary>
public enum BotSignal
{
    None,
    BotUserAgent,       // Known automation / MCP user-agent
    HoneypotTriggered,  // Bot filled a hidden field humans never see
    MissingJsToken,     // JS token absent — client skipped JS execution
    TooFast,            // Form submitted faster than any human could
    RateLimited         // Too many attempts from this IP in a short window
}

/// <summary>
/// Result of a bot-detection analysis.
/// </summary>
public record BotDetectionResult(bool IsBot, BotSignal Signal, string Reason);

/// <summary>
/// Detects automated / bot traffic on the login form using multiple passive signals:
/// user-agent analysis, honeypot field, JavaScript token, timing, and IP rate-limiting.
/// </summary>
public class BotDetectionService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<BotDetectionService> _logger;
    private readonly bool _enabled;

    // Minimum elapsed time (ms) between page load and form submit for a human
    private const int MinHumanResponseMs = 800;

    // Max failed or suspicious login attempts per IP before hard-blocking
    private const int MaxAttemptsPerWindow = 10;
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(15);

    /// <summary>
    /// User-agent substrings associated with bots, HTTP libraries, MCP clients, and
    /// AI/LLM automation frameworks.
    /// </summary>
    private static readonly string[] BotAgentKeywords =
    [
        // Generic bots
        "bot", "crawler", "spider", "scraper",
        // Common HTTP clients used by automation
        "curl", "wget", "httpie", "libwww", "lwp-",
        // Language-level HTTP libraries
        "python-requests", "python-httpx", "python-urllib",
        "go-http-client", "java/", "okhttp", "ruby",
        "node-fetch", "got/", "axios", "superagent", "undici",
        // API / test tools
        "postman", "insomnia", "rest-client", "paw/",
        // Headless browsers and UI automation
        "headlesschrome", "phantomjs", "selenium", "playwright", "puppeteer",
        // MCP / AI agent frameworks
        "mcp", "model-context-protocol",
        "openai-", "anthropic-", "cohere-",
        "llm-agent", "aiagent", "autogpt", "langchain"
    ];

    public BotDetectionService(IMemoryCache cache, ILogger<BotDetectionService> logger, IConfiguration configuration)
    {
        _cache = cache;
        _logger = logger;
        _enabled = configuration.GetValue<bool>("BotDetection:Enabled", defaultValue: true);
    }

    /// <summary>
    /// Analyses the current request and form fields to determine whether the login
    /// attempt originates from an automated client.
    /// </summary>
    /// <param name="context">Current HTTP context.</param>
    /// <param name="honeypot">Value of the hidden honeypot field (must be empty).</param>
    /// <param name="captchaToken">Value set by client-side JavaScript; absent when JS is not executed.</param>
    /// <param name="pageLoadTimeStr">Unix timestamp (ms) recorded by JS when the page loaded.</param>
    public BotDetectionResult Analyze(
        HttpContext context,
        string? honeypot,
        string? captchaToken,
        string? pageLoadTimeStr)
    {
        // Bypass all checks when bot detection is turned off in config
        if (!_enabled)
            return new(false, BotSignal.None, string.Empty);

        var userAgent = context.Request.Headers.UserAgent.ToString();
        var ip = GetClientIp(context);

        // 1. User-Agent inspection — empty UA or known automation keyword
        if (IsBotUserAgent(userAgent))
        {
            _logger.LogWarning(
                "[BotDetection] Bot UA from {Ip} — Agent: \"{UA}\"", ip, userAgent);
            return new(true, BotSignal.BotUserAgent, "Automated client detected.");
        }

        // 2. Honeypot — human visitors never fill this CSS-hidden field
        if (!string.IsNullOrEmpty(honeypot))
        {
            _logger.LogWarning(
                "[BotDetection] Honeypot triggered from {Ip}", ip);
            return new(true, BotSignal.HoneypotTriggered, "Automated client detected.");
        }

        // 3. JavaScript token — must equal "ok", written only by the login page script
        if (string.IsNullOrEmpty(captchaToken) || captchaToken != "ok")
        {
            _logger.LogWarning(
                "[BotDetection] Missing/invalid JS token from {Ip} — Agent: \"{UA}\"", ip, userAgent);
            return new(true, BotSignal.MissingJsToken,
                "JavaScript must be enabled to sign in.");
        }

        // 4. Timing — bots respond in milliseconds; humans take at least a second
        if (long.TryParse(pageLoadTimeStr, out var pageLoadMs))
        {
            var elapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - pageLoadMs;
            if (elapsed < MinHumanResponseMs)
            {
                _logger.LogWarning(
                    "[BotDetection] Too-fast submission ({Ms} ms) from {Ip}", elapsed, ip);
                return new(true, BotSignal.TooFast, "Automated client detected.");
            }
        }

        // 5. IP rate-limit — cap login attempts per IP within the rolling window
        if (IsRateLimited(ip))
        {
            _logger.LogWarning(
                "[BotDetection] Rate limit exceeded for {Ip}", ip);
            return new(true, BotSignal.RateLimited,
                "Too many login attempts. Please try again later.");
        }

        return new(false, BotSignal.None, string.Empty);
    }

    /// <summary>
    /// Increments the failed-attempt counter for the requesting IP.
    /// Call this after every failed login to feed the rate limiter.
    /// </summary>
    public void RecordFailedAttempt(HttpContext context)
    {
        var ip = GetClientIp(context);
        var key = CacheKey(ip);
        var current = _cache.Get<int?>(key) ?? 0;
        _cache.Set(key, current + 1, RateLimitWindow);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private bool IsRateLimited(string ip)
    {
        var count = _cache.Get<int?>(CacheKey(ip)) ?? 0;
        return count >= MaxAttemptsPerWindow;
    }

    private static bool IsBotUserAgent(string ua)
    {
        if (string.IsNullOrWhiteSpace(ua)) return true; // no UA → treat as bot

        var lower = ua.ToLowerInvariant();
        return BotAgentKeywords.Any(k => lower.Contains(k));
    }

    private static string CacheKey(string ip) => $"bot_login_{ip}";

    private static string GetClientIp(HttpContext context)
    {
        // Respect reverse-proxy header if present
        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
            return forwarded.Split(',')[0].Trim();

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
