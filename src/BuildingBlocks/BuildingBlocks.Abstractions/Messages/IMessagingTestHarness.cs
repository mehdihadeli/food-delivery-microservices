namespace BuildingBlocks.Abstractions.Messages;

public interface IMessagingTestHarness
{
    void AddConsumeMessage(IMessageEnvelopeBase messageEnvelopeBase);
    void AddPublishedMessage(IMessageEnvelopeBase messageEnvelopeBase);
    void WaitForPublishedMessage<T>()
        where T : IMessage;
    void WaitForConsumedMessage<T>()
        where T : IMessage;
    bool IsConsumedMessage<T>()
        where T : IMessage;
    bool IsPublishedMessage<T>()
        where T : IMessage;
}
