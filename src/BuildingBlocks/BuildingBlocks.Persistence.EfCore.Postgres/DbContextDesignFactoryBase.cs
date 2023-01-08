using BuildingBlocks.Core.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Persistence.EfCore.Postgres;

public abstract class DbContextDesignFactoryBase<TDbContext> : IDesignTimeDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    private readonly string _connectionStringSection;
    private readonly string? _env;

    protected DbContextDesignFactoryBase(string connectionStringSection, string? env = null)
    {
        _connectionStringSection = connectionStringSection;
        _env = env;
    }

    public TDbContext CreateDbContext(string[] args)
    {
        Console.WriteLine($"BaseDirectory: {AppContext.BaseDirectory}");

        var environmentName = _env ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "test";

        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory ?? "")
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environmentName}.json", true) // it is optional
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        var connectionStringSectionValue = configuration.GetValue<string>(_connectionStringSection);

        if (string.IsNullOrWhiteSpace(connectionStringSectionValue))
        {
            throw new InvalidOperationException($"Could not find a value for {_connectionStringSection} section.");
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
