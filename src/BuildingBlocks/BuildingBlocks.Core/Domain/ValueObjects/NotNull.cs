using BuildingBlocks.Core.Extensions;

namespace BuildingBlocks.Core.Domain.ValueObjects;

public record NotNull<T>
{
    protected NotNull(T value) => Value = value.NotBeNull();

    public T Value { get; }
}
