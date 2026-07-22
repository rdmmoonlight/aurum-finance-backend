using System.Net;

namespace Aurum.Api.Core.Exceptions;

/// <summary>
/// Thrown for a generic business-rule violation that isn't a conflict or a
/// missing resource. Translated into HTTP 400.
/// </summary>
public sealed class BadRequestException : AppException
{
    public BadRequestException(string message)
        : base(message, HttpStatusCode.BadRequest, "BAD_REQUEST")
    {
    }
}
