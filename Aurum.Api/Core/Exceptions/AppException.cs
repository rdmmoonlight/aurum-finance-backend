using System.Net;

namespace Aurum.Api.Core.Exceptions;

/// <summary>
/// Base type for all application-level exceptions that should be translated
/// into a predictable HTTP response by the global exception handling middleware.
/// </summary>
public abstract class AppException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public string ErrorCode { get; }

    protected AppException(string message, HttpStatusCode statusCode, string errorCode)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
