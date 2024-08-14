namespace BuildingBlocks.Core.Exception.Types;

public class InvalidEmailException(string email) : BadRequestException($"Email: '{email}' is invalid.")
{
    public string Email { get; } = email;
}
