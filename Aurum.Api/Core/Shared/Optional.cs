namespace Aurum.Api.Core.Shared;

/// <summary>
/// Distinguishes "this JSON key was not sent" from "this JSON key was sent
/// with value null" — the same distinction NestJS's <c>dto.field !== undefined</c>
/// checks relied on (see the original Update*Dto classes, built via
/// <c>PartialType</c>). A plain nullable C# property can't tell these two
/// cases apart; every "Update*Request" DTO migrated from a Nest PartialType
/// DTO should use this instead of a bare nullable property for any field
/// where "leave unchanged" and "explicitly clear" are different things.
///
/// Wire-up: registered via OptionalJsonConverterFactory in Program.cs. The
/// converter is only invoked when the JSON key is present, so IsSet is
/// naturally false when the key is missing and true whenever it's present
/// (including an explicit null) — no special-case handling needed.
/// </summary>
public readonly struct Optional<T>
{
    public bool IsSet { get; }

    public T? Value { get; }

    private Optional(bool isSet, T? value)
    {
        IsSet = isSet;
        Value = value;
    }

    public static Optional<T> Unset => default;

    public static Optional<T> Of(T? value) => new(true, value);
}
