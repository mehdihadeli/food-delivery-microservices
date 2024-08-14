namespace BuildingBlocks.Abstractions.Domain;

public interface IBusinessRuleWithExceptionType<out TException>
    where TException : Exception
{
    TException Exception { get; }
    bool IsBroken();
}
