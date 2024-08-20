namespace BuildingBlocks.Core.Messaging;

public class MessagingOptions
{
    public bool OutboxEnabled { get; set; } = true;
    public bool InboxEnabled { get; set; } = true;
    public bool AutoConfigEndpoints { get; set; }
}
