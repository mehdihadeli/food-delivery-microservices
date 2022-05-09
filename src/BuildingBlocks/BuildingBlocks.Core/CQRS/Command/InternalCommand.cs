using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Core.Types;

namespace BuildingBlocks.Core.CQRS.Command;

public abstract record InternalCommand : IInternalCommand
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public DateTime OccurredOn { get; protected set; } = DateTime.Now;

    public string Type { get { return TypeMapper.GetTypeName(GetType()); } }
}
