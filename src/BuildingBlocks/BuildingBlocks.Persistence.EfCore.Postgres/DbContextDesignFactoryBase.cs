using BuildingBlocks.Core.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Persistence.EfCore.Postgres;

public abstract class DbContextDesignFactoryBase<TDbContext>(string connectionStringSection, string? env = null)
    : IDesignTimeDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    public TDbContext CreateDbContext(string[] args)
    {
        Console.WriteLine($"BaseDirectory: {AppContext.BaseDirectory}");

        var environmentName = env ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "test";

        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory ?? "")
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environmentName}.json", true) // it is optional
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        var connectionStringSectionValue = configuration.GetValue<string>(connectionStringSection);

        if (string.IsNullOrWhiteSpace(connectionStringSectionValue))
        {
            throw new InvalidOperationException($"Could not find a value for {connectionStringSection} section.");
        }

        Console.WriteLine($"ConnectionString  section value is : {connectionStringSectionValue}");

        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>()
            .UseNpgsql(
                connectionStringSectionValue,
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(GetType().Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                }
            )
            .UseSnakeCaseNamingConvention()
            .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector<long>>();

        return (TDbContext)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options);
    }
}
