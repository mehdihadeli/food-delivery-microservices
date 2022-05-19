using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.Types;

namespace BuildingBlocks.Core.CQRS.Commands;

public abstract record InternalCommand : IInternalCommand
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public DateTime OccurredOn { get; protected set; } = DateTime.Now;

    public string Type { get { return TypeMapper.GetTypeName(GetType()); } }
}
