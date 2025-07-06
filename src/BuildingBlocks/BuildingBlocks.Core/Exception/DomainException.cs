using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Core.Exception;

// https://www.kamilgrzybek.com/blog/posts/domain-model-validation

/// <summary>
/// Exception type for domain exceptions.
/// </summary>
public class DomainException : CustomException
{
    private readonly Type? _brokenRuleType;

    public DomainException(string message, int statusCode = StatusCodes.Status400BadRequest)
        : base(message, statusCode) { }

    public DomainException(Type businessRuleType, string message, int statusCode = StatusCodes.Status400BadRequest)
        : base(message, statusCode)
    {
        _brokenRuleType = businessRuleType;
    }

    // Will use in the problem detail `title` field.
    public override string ToString()
    {
        if (_brokenRuleType is not null)
        {
            return $"{GetType().FullName}:{_brokenRuleType.FullName}";
        }

        return $"{GetType().FullName}";
    }
}
