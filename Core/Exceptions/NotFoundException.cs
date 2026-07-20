using System.Net;

namespace Aurum.Api.Core.Exceptions;

/// <summary>
/// Thrown when a requested resource does not exist.
/// Translated by the global exception handler into HTTP 404.
/// </summary>
public sealed class NotFoundException : AppException
{
    public NotFoundException(string message)
        : base(message, HttpStatusCode.NotFound, "RESOURCE_NOT_FOUND")
    {
    }

    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with identifier '{key}' was not found.", HttpStatusCode.NotFound, "RESOURCE_NOT_FOUND")
    {
    }
}
