namespace BuildingBlocks.Integration.MassTransit;

public class RabbitMqOptions
{
    public string Host { get; set; } = default!;
    public ushort Port { get; set; }
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string ConnectionString => $"amqp://{UserName}:{Password}@{Host}:{Port}/";
}
