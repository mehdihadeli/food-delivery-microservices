using BuildingBlocks.Abstractions.Serialization;
using MemoryPack;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Serialization.MemoryPack;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMemoryPackSerialization(
        this IServiceCollection services,
        Action<MemoryPackSerializerOptions>? configuration = null
    )
    {
        var serializerOptions = MemoryPackSerializerOptions.Default;

        configuration?.Invoke(serializerOptions);

        services.Replace(
            ServiceDescriptor.Transient<ISerializer>(_ => new MemoryPackObjectSerializer(serializerOptions))
        );
        services.Replace(
            ServiceDescriptor.Transient<IMessageSerializer>(_ => new MemoryPackMessageSerializer(serializerOptions))
        );

        return services;
    }
}
