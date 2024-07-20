using BuildingBlocks.Abstractions.Web.Problem;
using BuildingBlocks.Core.Domain.Exceptions;
using BuildingBlocks.Core.Exception.Types;
using BuildingBlocks.Validation;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Web.Problem;

internal sealed class DefaultProblemDetailMapper : IProblemDetailMapper
{
    public int GetMappedStatusCodes(Exception exception)
    {
        return exception switch
        {
            ConflictException conflictException => conflictException.StatusCode,
            ConflictAppException conflictException => conflictException.StatusCode,
            ConflictDomainException conflictException => conflictException.StatusCode,
            ValidationException validationException => validationException.StatusCode,
            ArgumentException _ => StatusCodes.Status400BadRequest,
            BadRequestException badRequestException => badRequestException.StatusCode,
            NotFoundException notFoundException => notFoundException.StatusCode,
            NotFoundDomainException notFoundException => notFoundException.StatusCode,
            NotFoundAppException notFoundException => notFoundException.StatusCode,
            HttpResponseException httpResponseException => httpResponseException.StatusCode,
            HttpRequestException httpRequestException => (int)httpRequestException.StatusCode,
            AppException appException => appException.StatusCode,
            DomainException domainException => domainException.StatusCode,
            _ => 0
        };
    }
}
