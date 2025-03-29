using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Web.Minimal;

public record HttpCommand<TRequest>(
    TRequest Request,
    HttpContext HttpContext,
    ICommandBus CommandBus,
    CancellationToken CancellationToken
) : IHttpCommand<TRequest>;
