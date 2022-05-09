using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Messaging.MessagePersistence.InMemory;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Core.Registrations;

public static partial class InMemoryMessagingRegistrationExtensions
{
    public static IServiceCollection AddInMemoryMessagePersistence(
        this IServiceCollection services,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        services.AddDbContext<InMemoryMessagePersistenceContext>(
            options => { options.UseInMemoryDatabase($"InMemoryOutbox_{Guid.NewGuid()}"); });

        services.Replace<IMessagePersistenceService, InMemoryMessagePersistenceService>(serviceLifetime);

        return services;
    }
}
