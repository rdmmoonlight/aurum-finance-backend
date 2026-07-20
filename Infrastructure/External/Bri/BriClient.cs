using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Aurum.Api.Core.Exceptions;

namespace Aurum.Api.Infrastructure.External.Bri;

/// <summary>OAuth2 token response shape from BRI's client_credentials endpoint.</summary>
internal sealed class BriTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    // BRIAPI tokens are valid ~50h; expires_in (seconds) may or may not be
    // present depending on product — we fall back to a safe default.
    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; set; }
}

/// <summary>
/// Ported 1:1 from NestJS's bank-account/bri.client.ts, moved to
/// Infrastructure/External per the third-party-integration migration step.
/// Real BRIAPI sandbox integration (https://developers.bri.co.id).
///
/// WHAT IS WIRED UP AND VERIFIED against BRI's public docs:
///   - OAuth2 client_credentials token exchange (GetAccessTokenAsync)
///   - BRI-Signature request signing (SignPayload), per the
///     "path + verb + token + timestamp + body" HMAC-SHA256 scheme
///
/// WHAT IS STILL A PLACEHOLDER (unchanged from the Nest original):
///   - InquireAccountAsync() / FetchMutationsAsync() below do NOT yet call
///     a real BRI business endpoint. BRIAPI splits balance inquiry and
///     mutation history across different products (Account Information,
///     Fund Transfer, SNAP, BRIVA, ...), each with its own path and
///     required scopes/subscription. Confirm which product this Client ID
///     is provisioned for (BRI, or the Postman collection issued alongside
///     the credentials) and the two TODOs below can be completed without
///     touching BankAccountService or the frontend.
///
/// Registered as a singleton (see BankAccountServiceExtensions) so the
/// cached OAuth2 token is shared across requests, same as Nest's
/// default-singleton @Injectable() scope. Uses IHttpClientFactory (a named
/// client, "Bri") rather than a raw `new HttpClient()` per call, to avoid
/// socket exhaustion — the one infrastructure difference from a literal
/// port; behavior is otherwise identical.
///
/// Credentials are read from BRI_BASE_URL / BRI_CLIENT_ID / BRI_CLIENT_SECRET
/// environment variables only — see .env.example. Never hardcode them.
/// </summary>
public sealed class BriClient : IBriClient
{
    private const string HttpClientName = "Bri";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BriClient> _logger;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    private (string AccessToken, DateTime ExpiresAtUtc)? _cachedToken;

    public BriClient(IHttpClientFactory httpClientFactory, ILogger<BriClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public bool IsConfigured => ReadConfig() is not null;

    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        var config = ReadConfig()
            ?? throw new ServiceUnavailableException("BRI_BASE_URL / BRI_CLIENT_ID / BRI_CLIENT_SECRET are not configured.");

        if (_cachedToken is { } cached && cached.ExpiresAtUtc > DateTime.UtcNow)
        {
            return cached.AccessToken;
        }

        await _tokenLock.WaitAsync(ct);
        try
        {
            // Re-check after acquiring the lock — another caller may have
            // already refreshed it while we were waiting.
            if (_cachedToken is { } recheck && recheck.ExpiresAtUtc > DateTime.UtcNow)
            {
                return recheck.AccessToken;
            }

            var client = _httpClientFactory.CreateClient(HttpClientName);
            var url = $"{config.BaseUrl}/oauth/client_credential/accesstoken?grant_type=client_credentials";

            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = config.ClientId,
                ["client_secret"] = config.ClientSecret,
            });

            using var response = await client.PostAsync(url, form, ct);

            if (!response.IsSuccessStatusCode)
            {
                var detail = await SafeReadAsync(response, ct);
                _logger.LogError("BRI token exchange failed ({StatusCode}): {Detail}", (int)response.StatusCode, detail);
                throw new ServiceUnavailableException("BRI did not accept the configured credentials.");
            }

            var payload = await response.Content.ReadFromJsonAsync<BriTokenResponse>(cancellationToken: ct);
            var accessToken = payload?.AccessToken;

            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ServiceUnavailableException("BRI token response did not include access_token.");
            }

            var ttlSeconds = payload!.ExpiresIn ?? 50 * 60 * 60; // BRIAPI default: ~50h
            _cachedToken = (accessToken, DateTime.UtcNow.AddSeconds(ttlSeconds - 300)); // refresh 5 min early

            return accessToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    public string SignPayload(string path, string verb, string token, string timestamp, string body)
    {
        var config = ReadConfig()
            ?? throw new ServiceUnavailableException("BRI credentials are not configured.");

        var payload = $"path={path}&verb={verb}&token={token}&timestamp={timestamp}&body={body}";
        var keyBytes = Encoding.UTF8.GetBytes(config.ClientSecret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        return Convert.ToBase64String(hmac.ComputeHash(payloadBytes));
    }

    // ============================================================
    // Business endpoints — TODO once the BRI product is confirmed.
    // ============================================================

    public async Task<BriInquiryResult> InquireAccountAsync(string accountHolderName, string accountNumber, CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            return MockInquiry(accountNumber);
        }

        // Confirms the credentials are live and valid. Throws
        // ServiceUnavailableException upstream if BRI rejects them.
        await GetAccessTokenAsync(ct);

        // TODO: replace with the real balance/account-info endpoint once
        // the subscribed BRI product is confirmed (e.g. GET /v1/balance,
        // or the Fund Transfer "internal/accounts" inquiry). Until then,
        // credentials are verified but the balance is still simulated so
        // the rest of the feature stays testable end-to-end.
        _logger.LogWarning(
            "BRI credentials verified for {AccountHolderName}, but no business endpoint is wired yet — returning a simulated balance.",
            accountHolderName);

        return MockInquiry(accountNumber);
    }

    public async Task<IReadOnlyList<BriMutation>> FetchMutationsAsync(string accountNumber, CancellationToken ct = default)
    {
        if (IsConfigured)
        {
            await GetAccessTokenAsync(ct);
            // TODO: replace with the real mutation/statement endpoint once
            // the subscribed BRI product is confirmed.
        }

        return MockMutations(accountNumber);
    }

    private static BriInquiryResult MockInquiry(string accountNumber)
    {
        var seed = ExtractSeed(accountNumber);
        var balance = 2_500_000m + (seed % 47) * 137_000m;
        return new BriInquiryResult(Verified: true, Balance: decimal.Round(balance, 2));
    }

    private static IReadOnlyList<BriMutation> MockMutations(string accountNumber)
    {
        var seed = ExtractSeed(accountNumber);
        var now = DateTime.UtcNow;

        (string Description, decimal Base, string Type)[] sample =
        {
            ("Transfer masuk - Payroll", 3_250_000m, "credit"),
            ("QRIS - Toko Kelontong", 35_000m, "debit"),
            ("Admin Bank Bulanan", 12_500m, "debit"),
            ("Transfer keluar - Listrik/PLN", 250_000m, "debit"),
            ("Transfer masuk - Refund", 75_000m, "credit"),
        };

        return sample.Select((s, i) => new BriMutation(
            TransactionDateUtc: now.AddDays(-(i + 1)),
            Description: s.Description,
            Amount: decimal.Round(s.Base + (seed % 13) * 1_000m, 2),
            Type: s.Type)).ToList();
    }

    private static long ExtractSeed(string accountNumber)
    {
        var tail = accountNumber.Length > 6 ? accountNumber[^6..] : accountNumber;
        return long.TryParse(tail, out var value) ? value : 0;
    }

    private static BriConfig? ReadConfig()
    {
        var baseUrl = Environment.GetEnvironmentVariable("BRI_BASE_URL");
        var clientId = Environment.GetEnvironmentVariable("BRI_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("BRI_CLIENT_SECRET");

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            return null;
        }

        return new BriConfig(baseUrl, clientId, clientSecret);
    }

    private static async Task<string> SafeReadAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            return await response.Content.ReadAsStringAsync(ct);
        }
        catch
        {
            return string.Empty;
        }
    }
}
