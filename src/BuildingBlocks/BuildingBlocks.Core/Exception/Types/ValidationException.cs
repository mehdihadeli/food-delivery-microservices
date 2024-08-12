namespace BuildingBlocks.Core.Exception.Types;

public class ValidationException(string message, System.Exception? innerException = null, params string[] errors)
    : BadRequestException(message, innerException, errors);
