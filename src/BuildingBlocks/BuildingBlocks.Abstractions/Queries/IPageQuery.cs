using BuildingBlocks.Abstractions.Core.Paging;

namespace BuildingBlocks.Abstractions.Queries;

public interface IPageQuery<out TResponse> : IPageRequest, IQuery<TResponse>
    where TResponse : notnull;
