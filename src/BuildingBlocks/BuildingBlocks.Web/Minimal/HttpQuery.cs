using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Web.Minimal;

public record HttpQuery(HttpContext HttpContext, IQueryBus QueryBus, CancellationToken CancellationToken) : IHttpQuery;
