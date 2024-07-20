using Marten.Schema.Identity;

namespace BuildingBlocks.Persistence.Marten;

public static class MartenIdGenerator
{
    public static Guid New() => CombGuidIdGeneration.NewGuid();
}
