using BuildingBlocks.Core.Extensions;

namespace BuildingBlocks.Core.Domain.ValueObjects;

public record NotEmptyGuid
{
    protected NotEmptyGuid(Guid value) => Value = value.NotBeEmpty();

    public Guid Value { get; }
}
