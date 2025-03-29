namespace BuildingBlocks.Core.Messages;

public class MessagingOptions
{
    public bool OutboxEnabled { get; set; } = true;
    public bool InboxEnabled { get; set; } = true;
}
