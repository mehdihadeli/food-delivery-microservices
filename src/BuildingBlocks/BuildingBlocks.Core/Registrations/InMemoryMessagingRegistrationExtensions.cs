using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Core.Messaging.MessagePersistence.InMemory;
using BuildingBlocks.Core.Web.Extenions.ServiceCollection;

namespace BuildingBlocks.Core.Registrations;

public static class InMemoryMessagingRegistrationExtensions
{
    public static IServiceCollection AddInMemoryMessagePersistence(this IServiceCollection services)
    {
        services.ReplaceScoped<IMessagePersistenceRepository, InMemoryMessagePersistenceRepository>();

        return services;
    }
}
