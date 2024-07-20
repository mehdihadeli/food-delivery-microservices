using BuildingBlocks.Core.Exception.Types;

namespace BuildingBlocks.Core.Domain.Exceptions;

public class InvalidCurrencyException : BadRequestException
{
    public string Currency { get; }

    public InvalidCurrencyException(string currency)
        : base($"Currency: '{currency}' is invalid.")
    {
        Currency = currency;
    }
}
