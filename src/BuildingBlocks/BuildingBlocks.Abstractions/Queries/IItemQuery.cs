namespace BuildingBlocks.Abstractions.Queries;

public interface IItemQuery<out TId, out TResponse> : IQuery<TResponse>
    where TId : struct
    where TResponse : notnull
{
    public IList<string> Includes { get; }
    public TId Id { get; }
}
