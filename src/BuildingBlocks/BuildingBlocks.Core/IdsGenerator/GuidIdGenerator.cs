using BuildingBlocks.Abstractions.Core;

namespace BuildingBlocks.Core.IdsGenerator;

public class GuidIdGenerator : IIdGenerator<Guid>
{
    public Guid New()
    {
       return Guid.NewGuid();
    }
}
