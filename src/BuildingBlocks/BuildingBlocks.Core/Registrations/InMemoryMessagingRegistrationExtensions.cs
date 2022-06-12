using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Messaging.MessagePersistence.InMemory;

namespace BuildingBlocks.Core.Registrations;

public static partial class InMemoryMessagingRegistrationExtensions
{
    public static IServiceCollection AddInMemoryMessagePersistence(
        this IServiceCollection services,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        services.Add<IMessagePersistenceRepository, InMemoryMessagePersistenceRepository>(serviceLifetime);

        services.Replace<IMessagePersistenceService, InMemoryMessagePersistenceService>(serviceLifetime);

        return services;
    }
}
