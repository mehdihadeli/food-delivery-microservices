using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Web.Minimal;

public record HttpQuery(
    HttpContext HttpContext,
    IQueryProcessor QueryProcessor,
    IMapper Mapper,
    CancellationToken CancellationToken
) : IHttpQuery;
