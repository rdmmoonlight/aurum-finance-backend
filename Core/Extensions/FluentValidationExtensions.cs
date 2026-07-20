using Aurum.Api.Core.Exceptions;
using FluentValidation;

namespace Aurum.Api.Core.Extensions;

/// <summary>
/// Bridges FluentValidation (registered per-DTO via AddValidatorsFromAssembly
/// in Program.cs) into the project's own exception-driven error pipeline.
/// MediatR is intentionally not part of this project (see README), so there
/// is no automatic ValidationBehavior pipeline step — every controller
/// action calls this explicitly instead, right after model binding and
/// before the DTO reaches its service.
/// </summary>
public static class FluentValidationExtensions
{
    public static async Task ValidateAndThrowAppExceptionAsync<T>(
        this IValidator<T> validator,
        T instance,
        CancellationToken ct = default)
    {
        var result = await validator.ValidateAsync(instance, ct);
        if (result.IsValid)
        {
            return;
        }

        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        throw new ValidationAppException(errors);
    }
}
