using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Core.Domain;
using BuildingBlocks.Core.IdsGenerator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Core.Persistence.EfCore.Interceptors;

public class GuidIdGeneratorInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        GenerateId(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        GenerateId(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void GenerateId(DbContext? context)
    {
        if (context == null)
            return;

        foreach (var entry in context.ChangeTracker.Entries<Entity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.Id.Value <= 0)
                    {
                        entry.CurrentValues[nameof(Entity.Id)] = EntityId.Of(SnowFlakIdGenerator.NewId());
                    }
                    break;
            }
        }
    }
}
