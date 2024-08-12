namespace BuildingBlocks.Core.Exception.Types;

public class InvalidCurrencyException(string currency) : BadRequestException($"Currency: '{currency}' is invalid.")
{
    public string Currency { get; } = currency;
}
