namespace BuildingBlocks.Abstractions.Messaging.PersistMessage;

public enum MessageDeliveryType
{
    Outbox = 1,
    Inbox = 2,
    Internal = 3
}
