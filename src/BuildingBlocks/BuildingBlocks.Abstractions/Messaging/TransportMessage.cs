namespace BuildingBlocks.Abstractions.Messaging;

public record TransportMessage(byte[]? Body, IDictionary<string, object?>? Headers);
