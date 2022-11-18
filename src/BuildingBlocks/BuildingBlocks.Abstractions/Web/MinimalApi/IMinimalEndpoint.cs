using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.CQRS.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BuildingBlocks.Abstractions.Web.MinimalApi;

public interface IMinimalEndpoint
{
    string GroupName { get; }
    string PrefixRoute { get; }
    double Version { get; }
    RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder);
}

public interface IMinimalEndpoint<TResult> : IMinimalEndpoint
{
    Task<TResult> HandleAsync(HttpContext context);
}

public interface IMinimalEndpoint<in TRequest, TResult> : IMinimalEndpoint
{
    Task<TResult> HandleAsync(HttpContext context, TRequest request, CancellationToken cancellationToken);
}

public interface IMinimalEndpoint<in TRequest, in TDependency, TResult> : IMinimalEndpoint
{
    Task<TResult> HandleAsync(
        HttpContext context,
        TRequest request,
        TDependency dependency,
        CancellationToken cancellationToken);
}

public interface IMinimalEndpoint<in TRequest, in TDependency1, in TDependency2, TResult> :
    IMinimalEndpoint
{
    Task<TResult> HandleAsync(
        HttpContext context,
        TRequest request,
        TDependency1 dependency1,
        TDependency2 dependency2,
        CancellationToken cancellationToken);
}

public interface
    IMinimalEndpoint<in TRequest, in TDependency1, in TDependency2, in TDependency3, TResult> :
        IMinimalEndpoint
{
    Task<TResult> HandleAsync(
        HttpContext context,
        TRequest request,
        TDependency1 dependency1,
        TDependency2 dependency2,
        TDependency3 dependency3,
        CancellationToken cancellationToken);
}

public interface ICommandMinimalEndpoint<in TRequest> : IMinimalEndpoint
{
    Task<IResult> HandleAsync(
        HttpContext context,
        TRequest request,
        ICommandProcessor commandProcessor,
        IMapper mapper,
        CancellationToken cancellationToken);
}

public interface ICommandMinimalEndpoint<in TRequest, TResult> : IMinimalEndpoint
{
    Task<TResult> HandleAsync(
        HttpContext context,
        TRequest request,
        ICommandProcessor commandProcessor,
        IMapper mapper,
        CancellationToken cancellationToken);
}

public interface ICommandMinimalEndpoint<in TRequest, in TDependency, TResult> : IMinimalEndpoint
{
    Task<TResult> HandleAsync(
        HttpContext context,
        TRequest request,
        ICommandProcessor commandProcessor,
        IMapper mapper,
        TDependency dependency1,
        CancellationToken cancellationToken);
}

public interface ICommandMinimalEndpoint<in TRequest, in TDependency1, in TDependency2, TResult> : IMinimalEndpoint
{
    Task<TResult> HandleAsync(
        HttpContext context,
        TRequest request,
        ICommandProcessor commandProcessor,
        IMapper mapper,
        TDependency1 dependency1,
        TDependency2 dependency2,
        CancellationToken cancellationToken);
}

public interface IQueryMinimalEndpoint<in TRequest> : IMinimalEndpoint
{
    Task<IResult> HandleAsync(
        HttpContext context,
        TRequest request,
        IQueryProcessor queryProcessor,
        IMapper mapper,
        CancellationToken cancellationToken);
}

public interface IQueryMinimalEndpoint<in TRequest, TResult> : IMinimalEndpoint
{
    Task<TResult> HandleAsync(
        HttpContext context,
        TRequest request,
        IQueryProcessor queryProcessor,
        IMapper mapper,
        CancellationToken cancellationToken);
}

public interface IQueryMinimalEndpoint<in TRequest, in TDependency, TResult> : IMinimalEndpoint
{
    Task<TResult> HandleAsync(
        HttpContext context,
        TRequest request,
        IQueryProcessor queryProcessor,
        IMapper mapper,
        TDependency dependency,
        CancellationToken cancellationToken);
}

public interface IQueryMinimalEndpoint<in TRequest, in TDependency1, in TDependency2, TResult> : IMinimalEndpoint
{
    Task<TResult> HandleAsync(
        HttpContext context,
        TRequest request,
        IQueryProcessor queryProcessor,
        IMapper mapper,
        TDependency1 dependency1,
        TDependency2 dependency2,
        CancellationToken cancellationToken);
}
