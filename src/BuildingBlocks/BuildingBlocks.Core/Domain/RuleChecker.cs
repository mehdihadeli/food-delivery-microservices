using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Core.Domain.Exceptions;
using BuildingBlocks.Core.Exception;

namespace BuildingBlocks.Core.Domain;

public static class RuleChecker
{
    public static void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
            throw new BusinessRuleValidationException(rule);
    }

    public static void CheckRule<TException>(IBusinessRuleWithExceptionType<TException> rule)
        where TException : System.Exception
    {
        if (rule.IsBroken())
            throw rule.Exception;
    }
}
