using EAEmployee.Net8.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace EAEmployee.Tests.Tests.Unit;

/// <summary>
/// Unit tests for <see cref="BotDetectionService"/>. Each test stands up a
/// real <see cref="IMemoryCache"/> and a real <see cref="DefaultHttpContext"/>
/// so the public <c>Analyze</c> path is exercised end-to-end without
/// touching the network.
/// </summary>
[TestFixture]
[Category("Unit")]
[Parallelizable(ParallelScope.All)]
public class BotDetectionServiceTests
{
    private static BotDetectionService BuildService(
        IMemoryCache cache,
        bool enabled = true,
        IDictionary<string, string?>? configOverrides = null)
    {
        var configData = new Dictionary<string, string?>
        {
            ["BotDetection:Enabled"] = enabled.ToString()
        };
        if (configOverrides is not null)
        {
            foreach (var (k, v) in configOverrides) configData[k] = v;
        }

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        return new BotDetectionService(cache, NullLogger<BotDetectionService>.Instance, config);
    }

    private static DefaultHttpContext NewContext(string userAgent = "Mozilla/5.0 (Test)", string ip = "127.0.0.1")
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["User-Agent"] = userAgent;
        ctx.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(ip);
        return ctx;
    }

    // ── 1. Disabled toggle ──────────────────────────────────────────────────

    [Test]
    public void Disabled_Config_Bypasses_All_Other_Checks()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = BuildService(cache, enabled: false);

        // Even with an obvious bot UA, the disabled toggle should let it through.
        var result = service.Analyze(NewContext("curl/8.0.1"), honeypot: "x", captchaToken: null, pageLoadTimeStr: null);
        result.IsBot.Should().BeFalse();
        result.Signal.Should().Be(BotSignal.None);
    }

    // ── 2. User-Agent inspection ────────────────────────────────────────────

    [TestCase("curl/8.0.1")]
    [TestCase("python-requests/2.31")]
    [TestCase("HeadlessChrome")]
    [TestCase("Mozilla/5.0 (compatible; GPTBot)")]
    [TestCase("")] // empty UA is also treated as bot
    public void Known_Automation_UserAgent_Is_Flagged(string ua)
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = BuildService(cache);

        var result = service.Analyze(NewContext(ua), null, "ok", DateTimeOffset.UtcNow.AddSeconds(-5).ToUnixTimeMilliseconds().ToString());
        result.IsBot.Should().BeTrue("UA \"{0}\" matches a known bot signature", ua);
        result.Signal.Should().Be(BotSignal.BotUserAgent);
    }

    [Test]
    public void Normal_Browser_UserAgent_Is_Not_Flagged()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = BuildService(cache);

        var result = service.Analyze(
            NewContext("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Safari/537.36"),
            null,
            "ok",
            DateTimeOffset.UtcNow.AddSeconds(-5).ToUnixTimeMilliseconds().ToString());
        result.IsBot.Should().BeFalse();
    }

    // ── 3. Honeypot ─────────────────────────────────────────────────────────

    [Test]
    public void Honeypot_Trigger_Is_Flagged()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = BuildService(cache);

        var result = service.Analyze(NewContext(), honeypot: "https://spam.example.com", captchaToken: "ok",
            pageLoadTimeStr: DateTimeOffset.UtcNow.AddSeconds(-5).ToUnixTimeMilliseconds().ToString());
        result.IsBot.Should().BeTrue();
        result.Signal.Should().Be(BotSignal.HoneypotTriggered);
    }

    [Test]
    public void Empty_Honeypot_Does_Not_Trigger()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = BuildService(cache);

        var result = service.Analyze(NewContext(), honeypot: "", captchaToken: "ok",
            pageLoadTimeStr: DateTimeOffset.UtcNow.AddSeconds(-5).ToUnixTimeMilliseconds().ToString());
        result.Signal.Should().NotBe(BotSignal.HoneypotTriggered);
    }

    // ── 4. JS challenge token ───────────────────────────────────────────────

    [Test]
    public void Missing_Js_Token_Is_Flagged()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = BuildService(cache);

        var result = service.Analyze(NewContext(), null, captchaToken: null,
            pageLoadTimeStr: DateTimeOffset.UtcNow.AddSeconds(-5).ToUnixTimeMilliseconds().ToString());
        result.IsBot.Should().BeTrue();
        result.Signal.Should().Be(BotSignal.MissingJsToken);
    }

    [Test]
    public void Invalid_Js_Token_Is_Flagged()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = BuildService(cache);

        var result = service.Analyze(NewContext(), null, captchaToken: "yes",
            pageLoadTimeStr: DateTimeOffset.UtcNow.AddSeconds(-5).ToUnixTimeMilliseconds().ToString());
        result.IsBot.Should().BeTrue();
        result.Signal.Should().Be(BotSignal.MissingJsToken);
    }

    [Test]
    public void Ok_Js_Token_Is_Accepted()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = BuildService(cache);

        var result = service.Analyze(NewContext(), null, captchaToken: "ok",
            pageLoadTimeStr: DateTimeOffset.UtcNow.AddSeconds(-5).ToUnixTimeMilliseconds().ToString());
        result.Signal.Should().NotBe(BotSignal.MissingJsToken);
    }

    // ── 5. Timing ───────────────────────────────────────────────────────────

    [Test]
    public void Submission_Faster_Than_Threshold_Is_Flagged()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = BuildService(cache);

        // Page "loaded" only 100 ms ago — well below the 800 ms minimum.
        var pageLoad = DateTimeOffset.UtcNow.AddMilliseconds(-100).ToUnixTimeMilliseconds().ToString();
        var result = service.Analyze(NewContext(), null, "ok", pageLoad);
        result.IsBot.Should().BeTrue();
        result.Signal.Should().Be(BotSignal.TooFast);
    }

    [Test]
    public void Submission_Slower_Than_Threshold_Is_Accepted()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = BuildService(cache);

        // 3 seconds ago — well above the 800 ms minimum.
        var pageLoad = DateTimeOffset.UtcNow.AddSeconds(-3).ToUnixTimeMilliseconds().ToString();
        var result = service.Analyze(NewContext(), null, "ok", pageLoad);
        result.Signal.Should().NotBe(BotSignal.TooFast);
    }

    [Test]
    public void Missing_PageLoad_Time_Skips_The_Timing_Check()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = BuildService(cache);

        var result = service.Analyze(NewContext(), null, "ok", pageLoadTimeStr: null);
        // Falls through to the rate-limit check, which is fresh, so not bot.
        result.Signal.Should().NotBe(BotSignal.TooFast);
        result.IsBot.Should().BeFalse();
    }

    // ── 6. Rate limit ───────────────────────────────────────────────────────

    [Test]
    public async Task Ten_Failed_Attempts_From_The_Same_Ip_Rate_Limit_The_Next_Call()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = BuildService(cache);

        var ctx = NewContext(ip: "10.0.0.42");
        for (var i = 0; i < 10; i++)
        {
            service.RecordFailedAttempt(ctx);
        }

        var pageLoad = DateTimeOffset.UtcNow.AddSeconds(-3).ToUnixTimeMilliseconds().ToString();
        var result = await Task.Run(() => service.Analyze(ctx, null, "ok", pageLoad));
        result.IsBot.Should().BeTrue();
        result.Signal.Should().Be(BotSignal.RateLimited);
    }

    [Test]
    public async Task Different_Ips_Have_Independent_Rate_Limit_Quotas()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = BuildService(cache);

        // Burn the quota on IP #1.
        var ip1 = NewContext(ip: "10.0.0.1");
        for (var i = 0; i < 10; i++) service.RecordFailedAttempt(ip1);

        // IP #2 should still be clean.
        var ip2 = NewContext(ip: "10.0.0.2");
        var pageLoad = DateTimeOffset.UtcNow.AddSeconds(-3).ToUnixTimeMilliseconds().ToString();
        var result = await Task.Run(() => service.Analyze(ip2, null, "ok", pageLoad));
        result.IsBot.Should().BeFalse();
    }

    [Test]
    public void X_Forwarded_For_Header_Is_Honoured_For_The_Rate_Limit_Key()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = BuildService(cache);

        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["X-Forwarded-For"] = "203.0.113.5, 10.0.0.1";
        ctx.Request.Headers["User-Agent"] = "Mozilla/5.0";
        ctx.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

        for (var i = 0; i < 10; i++) service.RecordFailedAttempt(ctx);

        var pageLoad = DateTimeOffset.UtcNow.AddSeconds(-3).ToUnixTimeMilliseconds().ToString();
        var result = service.Analyze(ctx, null, "ok", pageLoad);
        result.Signal.Should().Be(BotSignal.RateLimited,
            "the first X-Forwarded-For entry (203.0.113.5) should be the rate-limit key");
    }

    // ── 7. All checks pass ──────────────────────────────────────────────────

    [Test]
    public void Clean_Request_Passes_Every_Check()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = BuildService(cache);

        var pageLoad = DateTimeOffset.UtcNow.AddSeconds(-2).ToUnixTimeMilliseconds().ToString();
        var result = service.Analyze(
            NewContext("Mozilla/5.0 (Macintosh) AppleWebKit/537.36"),
            honeypot: null,
            captchaToken: "ok",
            pageLoadTimeStr: pageLoad);

        result.IsBot.Should().BeFalse();
        result.Signal.Should().Be(BotSignal.None);
        result.Reason.Should().BeEmpty();
    }
}
