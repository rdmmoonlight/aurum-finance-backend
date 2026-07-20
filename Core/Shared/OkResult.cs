namespace Aurum.Api.Core.Shared;

/// <summary>
/// Matches the NestJS backend's <c>{ ok: true }</c> response, returned by
/// every delete endpoint across the original API.
/// </summary>
public sealed class OkResult
{
    public bool Ok { get; init; } = true;
}
