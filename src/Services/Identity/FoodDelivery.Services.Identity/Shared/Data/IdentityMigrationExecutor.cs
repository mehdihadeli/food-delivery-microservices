using BuildingBlocks.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Identity.Shared.Data;

public class IdentityMigrationExecutor : IMigrationExecutor
{
    private readonly IdentityContext _identityContext;
    private readonly ILogger<IdentityMigrationExecutor> _logger;

    public IdentityMigrationExecutor(IdentityContext identityContext, ILogger<IdentityMigrationExecutor> logger)
    {
        _identityContext = identityContext;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Migration worker started");

        _logger.LogInformation("Updating identity database...");

        await _identityContext.Database.MigrateAsync(cancellationToken: cancellationToken);

        _logger.LogInformation("identity database Updated");
    }
}
