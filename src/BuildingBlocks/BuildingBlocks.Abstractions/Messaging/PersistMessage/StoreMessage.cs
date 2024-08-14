namespace BuildingBlocks.Abstractions.Messaging.PersistMessage;

public class StoreMessage(Guid id, string dataType, string data, MessageDeliveryType deliveryType)
{
    public Guid Id { get; private set; } = id;
    public string DataType { get; private set; } = dataType;
    public string Data { get; private set; } = data;
    public DateTime Created { get; private set; } = DateTime.Now;
    public int RetryCount { get; private set; }
    public MessageStatus MessageStatus { get; private set; } = MessageStatus.Stored;
    public MessageDeliveryType DeliveryType { get; private set; } = deliveryType;

    public void ChangeState(MessageStatus messageStatus)
    {
        MessageStatus = messageStatus;
    }

    public void IncreaseRetry()
    {
        RetryCount++;
    }
}
