using BuildingBlocks.Core.Extensions;

namespace BuildingBlocks.Core.Domain.ValueObjects;

public record NotNegativeOrZero
{
    public NotNegativeOrZero(int value) => Value = value.NotBeNegativeOrZero();

    public int Value { get; }
}
