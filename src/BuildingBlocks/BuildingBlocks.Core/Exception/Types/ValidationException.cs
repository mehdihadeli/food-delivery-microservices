using BuildingBlocks.Core.Exception.Types;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Validation;

public class ValidationException : BadRequestException
{
    public ValidationException(string message, Exception? innerException = null, params string[] errors)
        : base(message, innerException, errors) { }
}
