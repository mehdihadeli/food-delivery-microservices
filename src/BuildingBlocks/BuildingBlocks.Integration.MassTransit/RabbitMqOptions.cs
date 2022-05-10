namespace BuildingBlocks.Integration.MassTransit;

public class RabbitMqOptions
{
    public string Host { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
}
