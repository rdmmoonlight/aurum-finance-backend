using System.Net;

namespace Aurum.Api.Core.Exceptions;

/// <summary>
/// Thrown for a generic business-rule violation that isn't a conflict or a
/// missing resource (e.g. "period is closed", "cash + bank must be greater
/// than 0"). Mirrors NestJS's <c>BadRequestException</c> 1:1 — every place
/// the original backend threw one, the migrated code throws this.
/// Translated into HTTP 400.
/// </summary>
public sealed class BadRequestException : AppException
{
    public BadRequestException(string message)
        : base(message, HttpStatusCode.BadRequest, "BAD_REQUEST")
    {
    }
}
