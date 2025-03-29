using BuildingBlocks.Core.Extensions;

namespace BuildingBlocks.Core.Domain.ValueObjects;

public record NotEmptyOrNull
{
    protected NotEmptyOrNull(string value) => Value = value.NotBeEmptyOrNull();

    public string Value { get; }
}
