using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
        CancellationToken cancellationToken
    );
}

public interface IMinimalEndpoint<in TRequest, in TDependency1, in TDependency2, TResult> : IMinimalEndpoint
{
    Task<TResult> HandleAsync(
        HttpContext context,
        TRequest request,
        TDependency1 dependency1,
        TDependency2 dependency2,
        CancellationToken cancellationToken
    );
}

public interface IMinimalEndpoint<in TRequest, in TDependency1, in TDependency2, in TDependency3, TResult>
    : IMinimalEndpoint
{
    Task<TResult> HandleAsync(
        HttpContext context,
        TRequest request,
        TDependency1 dependency1,
        TDependency2 dependency2,
        TDependency3 dependency3,
        CancellationToken cancellationToken
    );
}

public interface ICommandMinimalEndpoint<in TRequest, in TRequestParameters> : IMinimalEndpoint
    where TRequestParameters : IHttpCommand<TRequest>
{
    Task<IResult> HandleAsync([AsParameters] TRequestParameters requestParameters);
}

public interface ICommandMinimalEndpoint<in TRequest, in TRequestParameters, TResult1> : IMinimalEndpoint
    where TRequestParameters : IHttpCommand<TRequest>
    where TResult1 : IResult
{
    Task<TResult1> HandleAsync([AsParameters] TRequestParameters requestParameters);
}

public interface ICommandMinimalEndpoint<in TRequest, in TRequestParameters, TResult1, TResult2> : IMinimalEndpoint
    where TRequestParameters : IHttpCommand<TRequest>
    where TResult1 : IResult
    where TResult2 : IResult
{
    Task<Results<TResult1, TResult2>> HandleAsync([AsParameters] TRequestParameters requestParameters);
}

public interface ICommandMinimalEndpoint<in TRequest, in TRequestParameters, TResult1, TResult2, TResult3>
    : IMinimalEndpoint
    where TRequestParameters : IHttpCommand<TRequest>
    where TResult1 : IResult
    where TResult2 : IResult
    where TResult3 : IResult
{
    Task<Results<TResult1, TResult2, TResult3>> HandleAsync([AsParameters] TRequestParameters requestParameters);
}

public interface ICommandMinimalEndpoint<in TRequest, in TRequestParameters, TResult1, TResult2, TResult3, TResult4>
    : IMinimalEndpoint
    where TRequestParameters : IHttpCommand<TRequest>
    where TResult1 : IResult
    where TResult2 : IResult
    where TResult3 : IResult
    where TResult4 : IResult
{
    Task<Results<TResult1, TResult2, TResult3, TResult4>> HandleAsync(
        [AsParameters] TRequestParameters requestParameters
    );
}

public interface IQueryMinimalEndpoint<in TRequestParameters> : IMinimalEndpoint
    where TRequestParameters : IHttpQuery
{
    Task<IResult> HandleAsync([AsParameters] TRequestParameters requestParameters);
}

public interface IQueryMinimalEndpoint<in TRequestParameters, TResult1> : IMinimalEndpoint
    where TRequestParameters : IHttpQuery
    where TResult1 : IResult
{
    Task<TResult1> HandleAsync([AsParameters] TRequestParameters requestParameters);
}

public interface IQueryMinimalEndpoint<in TRequestParameters, TResult1, TResult2> : IMinimalEndpoint
    where TRequestParameters : IHttpQuery
    where TResult1 : IResult
    where TResult2 : IResult
{
    Task<Results<TResult1, TResult2>> HandleAsync([AsParameters] TRequestParameters requestParameters);
}

public interface IQueryMinimalEndpoint<in TRequestParameters, TResult1, TResult2, TResult3> : IMinimalEndpoint
    where TRequestParameters : IHttpQuery
    where TResult1 : IResult
    where TResult2 : IResult
    where TResult3 : IResult
{
    Task<Results<TResult1, TResult2, TResult3>> HandleAsync([AsParameters] TRequestParameters requestParameters);
}

public interface IQueryMinimalEndpoint<in TRequestParameters, TResult1, TResult2, TResult3, TResult4> : IMinimalEndpoint
    where TRequestParameters : IHttpQuery
    where TResult1 : IResult
    where TResult2 : IResult
    where TResult3 : IResult
    where TResult4 : IResult
{
    Task<Results<TResult1, TResult2, TResult3, TResult4>> HandleAsync(
        [AsParameters] TRequestParameters requestParameters
    );
}
