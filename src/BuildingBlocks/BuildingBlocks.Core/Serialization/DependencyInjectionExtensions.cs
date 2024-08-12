using BuildingBlocks.Abstractions.Serialization;

namespace BuildingBlocks.Core.Serialization;

internal static class DependencyInjectionExtensions
{
    internal static void AddDefaultSerializer(this IServiceCollection services)
    {
        services.AddTransient<ISerializer, DefaultSerializer>();
        services.AddTransient<IMessageSerializer, DefaultMessageSerializer>();
    }
}
