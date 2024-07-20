using BuildingBlocks.Abstractions.Messaging;

namespace FoodDelivery.Services.Customers;

public class TestIntegrationConsumer : IIntegrationEventHandler<TestIntegration>
{
    public Task Handle(TestIntegration notification, CancellationToken cancellationToken)
    {
        Console.WriteLine("Data is: " + notification.Data);
        return Task.CompletedTask;
    }
}
