using BuildingBlocks.Abstractions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Core.Persistence.EfCore.Interceptors;

// https://khalidabuhakmeh.com/entity-framework-core-5-interceptors
// https://blog.jetbrains.com/dotnet/2023/06/14/how-to-implement-a-soft-delete-strategy-with-entity-framework-core/
// https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors#savechanges-interception
// Ref: https://www.meziantou.net/entity-framework-core-soft-delete-using-query-filters.htm
public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context == null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity is IHaveSoftDelete)
                        entry.CurrentValues["IsDeleted"] = false;
                    break;
                case EntityState.Deleted:
                    if (entry.Entity is IHaveSoftDelete)
                    {
                        // change state to modified to prevent delete and change operation to update.
                        entry.State = EntityState.Modified;
                        eventData.Context.Entry(entry.Entity).CurrentValues["IsDeleted"] = true;
                    }

                    break;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
