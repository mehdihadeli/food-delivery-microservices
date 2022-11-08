using BuildingBlocks.Core.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Persistence.EfCore.Postgres;

public abstract class DbContextDesignFactoryBase<TDbContext> : IDesignTimeDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    private readonly string _connectionStringSection;

    protected DbContextDesignFactoryBase(string connectionStringSection)
    {
        _connectionStringSection = connectionStringSection;
    }

    public TDbContext CreateDbContext(string[] args)
    {
        Console.WriteLine($"BaseDirectory: {AppContext.BaseDirectory}");

        var configuration = ConfigurationHelper.GetConfiguration(AppContext.BaseDirectory);
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
            ).UseSnakeCaseNamingConvention();

        return (TDbContext)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options);
    }
}
