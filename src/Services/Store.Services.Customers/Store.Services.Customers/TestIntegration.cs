using BuildingBlocks.Core.Messaging;

namespace Store.Services.Customers;

public record TestIntegration(string Data) : IntegrationEvent;
