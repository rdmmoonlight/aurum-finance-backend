namespace Aurum.Api.Core.Shared;

/// <summary>
/// Standard envelope for every successful API response, ensuring a
/// consistent shape across all feature modules.
/// </summary>
public sealed class ApiResponse<T>
{
    public bool Success { get; init; }

    public T? Data { get; init; }

    public string? Message { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };
}

/// <summary>
/// Standard envelope for API responses that carry no payload.
/// </summary>
public sealed class ApiResponse
{
    public bool Success { get; init; }

    public string? Message { get; init; }

    public static ApiResponse Ok(string? message = null) => new()
    {
        Success = true,
        Message = message
    };
}
