using System.Text.Json.Serialization;

namespace EAEmployee.Net8.Services;

/// <summary>
/// Validates Google reCAPTCHA v2 tokens by calling the Google siteverify API.
/// Registered as an <see cref="System.Net.Http.HttpClient"/> typed client so
/// it receives a pre-configured <see cref="System.Net.Http.HttpClient"/> from DI.
/// </summary>
public class CaptchaService
{
    private readonly HttpClient _http;
    private readonly string _secretKey;
    private readonly bool _enabled;

    private const string VerifyUrl = "https://www.google.com/recaptcha/api/siteverify";

    public CaptchaService(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        _secretKey = configuration["GoogleRecaptcha:SecretKey"] ?? string.Empty;
        _enabled = configuration.GetValue<bool>("BotDetection:Enabled", defaultValue: true);
    }

    /// <summary>
    /// Returns true when CAPTCHA is disabled in config, or when the reCAPTCHA
    /// response token from the browser is confirmed valid by Google.
    /// </summary>
    public async Task<bool> ValidateAsync(string? responseToken)
    {
        if (!_enabled) return true;
        if (string.IsNullOrEmpty(responseToken)) return false;

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["secret"]   = _secretKey,
            ["response"] = responseToken
        });

        var httpResponse = await _http.PostAsync(VerifyUrl, form);
        var result = await httpResponse.Content
            .ReadFromJsonAsync<RecaptchaVerifyResponse>();

        return result?.Success is true;
    }

    // Google's siteverify response shape (we only need the success flag)
    private sealed record RecaptchaVerifyResponse(
        [property: JsonPropertyName("success")] bool Success);
}
