using System.Net;

namespace Aurum.Api.Core.Exceptions;

/// <summary>
/// Thrown when input fails business or FluentValidation rules.
/// Translated by the global exception handler into HTTP 400, including
/// a per-field breakdown of validation errors.
/// </summary>
public sealed class ValidationAppException : AppException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationAppException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.", HttpStatusCode.BadRequest, "VALIDATION_ERROR")
    {
        Errors = errors.ToDictionary(e => e.Key, e => e.Value);
    }
}
