namespace Aurum.Api.Infrastructure.External.Bri;

/// <summary>
/// Read from BRI_BASE_URL / BRI_CLIENT_ID / BRI_CLIENT_SECRET environment
/// variables only (see .env.example) — never hardcode these in source,
/// same rule the original Nest client followed.
/// </summary>
public sealed record BriConfig(string BaseUrl, string ClientId, string ClientSecret);

public sealed record BriInquiryResult(bool Verified, decimal Balance);

public sealed record BriMutation(DateTime TransactionDateUtc, string Description, decimal Amount, string Type);
