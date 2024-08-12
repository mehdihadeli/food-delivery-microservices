namespace BuildingBlocks.Abstractions.Messaging;

public interface IMessageMetadataAccessor
{
    /// <summary>
    ///     Get CorrelationId from header storage and generate new if not exists.
    /// </summary>
    /// <returns>Guid.</returns>
    Guid GetCorrelationId();

    Guid? GetCautionId();
}
