namespace BuildingBlocks.Integration.MassTransit;

public class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public ushort Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ConnectionString => $"amqp://{UserName}:{Password}@{Host}:{Port}/";
}
