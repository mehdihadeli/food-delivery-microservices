namespace BuildingBlocks.Abstractions.Domain;

public interface IBusinessRule
{
    bool IsBroken();
    string Message { get; }
    int Status { get; }
}

public interface IBusinessRuleWithExceptionType<out TException>
    where TException : Exception
{
    bool IsBroken();
    TException Exception { get; }
}
