using BuildingBlocks.Abstractions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Core.Persistence.EfCore.Interceptors;

// https://khalidabuhakmeh.com/entity-framework-core-5-interceptors
// https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors#savechanges-interception
// Ref: https://www.meziantou.net/entity-framework-core-generate-tracking-columns.htm
public class AuditInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var now = DateTime.Now;

        // var userId = GetCurrentUser(); // TODO: Get current user
        foreach (var entry in eventData.Context.ChangeTracker.Entries<IHaveAudit>())
        {
            switch (entry.State)
            {
                case EntityState.Modified:
                    entry.CurrentValues[nameof(IHaveAudit.LastModified)] = now;
                    entry.CurrentValues[nameof(IHaveAudit.LastModifiedBy)] = 1;
                    break;
                case EntityState.Added:
                    entry.CurrentValues[nameof(IHaveAudit.Created)] = now;
                    entry.CurrentValues[nameof(IHaveAudit.CreatedBy)] = 1;
                    break;
            }
        }

        foreach (var entry in eventData.Context.ChangeTracker.Entries<IHaveCreator>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.CurrentValues[nameof(IHaveCreator.Created)] = now;
                entry.CurrentValues[nameof(IHaveCreator.CreatedBy)] = 1;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
