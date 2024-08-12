namespace BuildingBlocks.Core.Exception.Types;

public class InvalidAmountException(decimal amount) : BadRequestException($"Amount: '{amount}' is invalid.")
{
    public decimal Amount { get; } = amount;
}
