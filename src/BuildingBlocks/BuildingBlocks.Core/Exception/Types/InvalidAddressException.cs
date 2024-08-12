namespace BuildingBlocks.Core.Exception.Types;

public class InvalidAddressException(string address) : BadRequestException($"Address: '{address}' is invalid.")
{
    public string Address { get; } = address;
}
