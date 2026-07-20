using System.Net;

namespace Aurum.Api.Core.Exceptions;

/// <summary>
/// Thrown when a request conflicts with the current state of a resource
/// (for example, a duplicate unique field). Translated into HTTP 409.
/// </summary>
public sealed class ConflictException : AppException
{
    public ConflictException(string message)
        : base(message, HttpStatusCode.Conflict, "RESOURCE_CONFLICT")
    {
    }
}
