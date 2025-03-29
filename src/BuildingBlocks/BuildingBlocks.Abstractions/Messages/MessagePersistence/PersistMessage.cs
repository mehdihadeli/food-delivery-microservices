namespace BuildingBlocks.Abstractions.Messages.MessagePersistence;

public class PersistMessage(
    Guid id,
    Guid messageId,
    string dataType,
    string data,
    MessageDeliveryType deliveryType,
    MessageStatus messageStatus
)
{
    public Guid Id { get; private set; } = id;
    public Guid MessageId { get; private set; } = messageId;
    public string DataType { get; private set; } = dataType;
    public string Data { get; private set; } = data;
    public DateTime Created { get; private set; } = DateTime.Now;
    public int RetryCount { get; private set; }
    public MessageStatus MessageStatus { get; private set; } = messageStatus;
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
