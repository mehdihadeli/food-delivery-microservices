namespace BuildingBlocks.Core.Exception.Types;

public class InvalidPhoneNumberException(string phoneNumber)
    : BadRequestException($"PhoneNumber: '{phoneNumber}' is invalid.")
{
    public string PhoneNumber { get; } = phoneNumber;
}
