namespace BuildingBlocks.Email.Options;

public class MailKitOptions
{
    public required string Host { get; init; }
    public required int Port { get; init; }
    public required string UserName { get; init; }
    public required string Password { get; init; }
}
