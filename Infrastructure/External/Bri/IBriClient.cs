namespace Aurum.Api.Infrastructure.External.Bri;

public interface IBriClient
{
    /// <summary>True once BRI_BASE_URL / BRI_CLIENT_ID / BRI_CLIENT_SECRET are all set.</summary>
    bool IsConfigured { get; }

    /// <summary>OAuth2 client_credentials grant, cached in memory until ~5 minutes before expiry.</summary>
    Task<string> GetAccessTokenAsync(CancellationToken ct = default);

    /// <summary>Builds the BRI-Signature header value required on every call except the token endpoint.</summary>
    string SignPayload(string path, string verb, string token, string timestamp, string body);

    /// <summary>Verifies the account holder name/number pair and returns the balance.</summary>
    Task<BriInquiryResult> InquireAccountAsync(string accountHolderName, string accountNumber, CancellationToken ct = default);

    /// <summary>Returns recent mutations for the linked account.</summary>
    Task<IReadOnlyList<BriMutation>> FetchMutationsAsync(string accountNumber, CancellationToken ct = default);
}
