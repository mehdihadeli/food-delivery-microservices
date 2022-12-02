using BuildingBlocks.Abstractions.Persistence.Mongo;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Persistence.Mongo
{
    public static class Extensions
    {
        public static IServiceCollection AddMongoDbContext<TContext>(
            this IServiceCollection services)
            where TContext : MongoDbContext
        {
            return services.AddMongoDbContext<TContext, TContext>();
        }

        public static IServiceCollection AddMongoDbContext<TContextService, TContextImplementation>(
            this IServiceCollection services)
            where TContextService : IMongoDbContext
            where TContextImplementation : MongoDbContext, TContextService
        {
            services.AddValidatedOptions<MongoOptions>(nameof(MongoOptions));

            services.AddScoped(typeof(TContextService), typeof(TContextImplementation));
            services.AddScoped(typeof(TContextImplementation));

            services.AddScoped<IMongoDbContext>(sp => sp.GetRequiredService<TContextService>());

            services.AddTransient(typeof(IMongoRepository<,>), typeof(MongoRepository<,>));
            services.AddTransient(typeof(IMongoUnitOfWork<>), typeof(MongoUnitOfWork<>));

            return services;
        }
    }
}
