using BuildingBlocks.Abstractions.Commands;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Abstractions.Web.MinimalApi;

// https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct#user-defined-types
// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#positional-syntax-for-property-definition
// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#nondestructive-mutation
// https://alexanderzeitler.com/articles/deconstructing-a-csharp-record-with-properties/
public interface IHttpCommand<TRequest>
{
    TRequest Request { get; init; }
    HttpContext HttpContext { get; init; }
    ICommandBus CommandBus { get; init; }
    CancellationToken CancellationToken { get; init; }
}

public interface IHttpCommand
{
    HttpContext HttpContext { get; init; }
    ICommandBus CommandBus { get; init; }
    CancellationToken CancellationToken { get; init; }
}
