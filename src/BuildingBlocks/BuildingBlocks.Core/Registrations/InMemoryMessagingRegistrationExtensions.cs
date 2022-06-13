using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Messaging.MessagePersistence.InMemory;

namespace BuildingBlocks.Core.Registrations;

public static class InMemoryMessagingRegistrationExtensions
{
    public static IServiceCollection AddInMemoryMessagePersistence(this IServiceCollection services)
    {
        services.ReplaceScoped<IMessagePersistenceRepository, InMemoryMessagePersistenceRepository>();

        return services;
    }
}
