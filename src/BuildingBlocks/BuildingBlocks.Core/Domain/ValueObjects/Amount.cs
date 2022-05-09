using Ardalis.GuardClauses;
using BuildingBlocks.Core.Exception.Types;

namespace BuildingBlocks.Core.Domain.ValueObjects;

public record Amount
{
    public decimal Value { get; private set; }

    public static Amount? Null => null;

    public static Amount Create(decimal value)
    {
        if (value is < 0 or > 1000000)
        {
            throw new InvalidAmountException(value);
        }

        return new Amount { Value = value };
    }

    public static Amount Zero => Create(0);

    public static implicit operator Amount?(decimal? value) => value == null ? null : Create((decimal)value);

    public static implicit operator decimal?(Amount? value) => value?.Value;

    public static bool operator >(Amount a, Amount b) =>
        Guard.Against.Null(a?.Value, nameof(a)) > Guard.Against.Null(b?.Value, nameof(b));

    public static bool operator <(Amount a, Amount b) =>
        Guard.Against.Null(a?.Value, nameof(a)) < Guard.Against.Null(b?.Value, nameof(b));

    public static bool operator >=(Amount a, Amount b) =>
        Guard.Against.Null(a?.Value, nameof(a)) >= Guard.Against.Null(b?.Value, nameof(b));

    public static bool operator <=(Amount a, Amount b) =>
        Guard.Against.Null(a?.Value, nameof(a)) <= Guard.Against.Null(b?.Value, nameof(b));

    public static Amount operator +(Amount a, Amount b) =>
        (Guard.Against.Null(a?.Value, nameof(a)) + Guard.Against.Null(b?.Value, nameof(b)))!;

    public static Amount operator -(Amount a, Amount b) =>
        (Guard.Against.Null(a?.Value, nameof(a)) - Guard.Against.Null(b?.Value, nameof(b)))!;
}
