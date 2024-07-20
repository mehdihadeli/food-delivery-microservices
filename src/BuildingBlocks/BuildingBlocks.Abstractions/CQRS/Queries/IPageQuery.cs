using BuildingBlocks.Abstractions.Core.Paging;

namespace BuildingBlocks.Abstractions.CQRS.Queries;

public interface IPageQuery<out TResponse> : IPageRequest, IQuery<TResponse>
    where TResponse : notnull { }
