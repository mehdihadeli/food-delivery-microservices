namespace BuildingBlocks.Abstractions.Domain;

public interface IBusinessRule
{
    bool IsBroken();
    string Message { get; }
}
