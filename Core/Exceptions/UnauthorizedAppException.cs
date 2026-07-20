using System.Net;

namespace Aurum.Api.Core.Exceptions;

/// <summary>
/// Thrown when authentication credentials are missing or invalid.
/// Translated by the global exception handler into HTTP 401.
/// </summary>
public sealed class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message = "Authentication is required or credentials are invalid.")
        : base(message, HttpStatusCode.Unauthorized, "UNAUTHORIZED")
    {
    }
}
