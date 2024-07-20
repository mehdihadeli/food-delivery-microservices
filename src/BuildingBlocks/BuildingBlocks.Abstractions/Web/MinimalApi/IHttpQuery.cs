using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Abstractions.Web.MinimalApi;

// https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct#user-defined-types
// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#positional-syntax-for-property-definition
// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#nondestructive-mutation
// https://alexanderzeitler.com/articles/deconstructing-a-csharp-record-with-properties/
public interface IHttpQuery
{
    HttpContext HttpContext { get; init; }
    IQueryProcessor QueryProcessor { get; init; }
    IMapper Mapper { get; init; }
    CancellationToken CancellationToken { get; init; }
}
