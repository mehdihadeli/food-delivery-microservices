namespace BuildingBlocks.Abstractions.CQRS.Queries;

public interface IListQuery<out TResponse> : IPageRequest, IQuery<TResponse>
    where TResponse : notnull
{
}
