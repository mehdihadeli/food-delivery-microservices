namespace BuildingBlocks.Abstractions.Core;

public interface IIdGenerator<out TId>
{
    TId New();
}
