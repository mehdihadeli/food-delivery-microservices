namespace BuildingBlocks.Core.Queries;

using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Core.Paging;

// https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/records#characteristics-of-records
public record PageQuery<TResponse> : PageRequest, IPageQuery<TResponse>
    where TResponse : notnull;
