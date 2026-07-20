using System.Net;

namespace Aurum.Api.Core.Exceptions;

/// <summary>Mirrors NestJS's ServiceUnavailableException — used when an external dependency (BRI) is unreachable or misconfigured.</summary>
public sealed class ServiceUnavailableException : AppException
{
    public ServiceUnavailableException(string message)
        : base(message, HttpStatusCode.ServiceUnavailable, "SERVICE_UNAVAILABLE")
    {
    }
}
