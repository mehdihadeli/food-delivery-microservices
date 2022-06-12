namespace BuildingBlocks.Abstractions.Messaging.PersistMessage;

[Flags]
public enum MessageDeliveryType
{
    Outbox = 1,
    Inbox = 2,
    Internal = 4
}
