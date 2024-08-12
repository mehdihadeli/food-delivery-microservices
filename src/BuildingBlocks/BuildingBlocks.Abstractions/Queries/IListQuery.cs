using BuildingBlocks.Abstractions.Core.Paging;

namespace BuildingBlocks.Abstractions.Queries;

public interface IListQuery<out TResponse> : IPageRequest, IQuery<TResponse>
    where TResponse : notnull { }
