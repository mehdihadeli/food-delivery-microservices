using BuildingBlocks.Abstractions.Domain;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Core.Persistence.EfCore.Interceptors;

// https://khalidabuhakmeh.com/entity-framework-core-5-interceptors
// https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors#savechanges-interception
public class ConcurrencyInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context == null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        foreach (var entry in eventData.Context.ChangeTracker.Entries<IAggregateBase>())
        {
            // https://dateo-software.de/blog/concurrency-entity-framework
            // https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations#application-managed-concurrency-tokens
            // application managed concurrency token, and it should use with `IsConcurrencyToken = true` which use for application level concurrency token handling
            var events = entry.Entity.GetUncommittedDomainEvents();
            if (events.Any())
            {
                if (entry.Entity is IHaveAggregateVersion av)
                {
                    entry.CurrentValues[nameof(IHaveAggregateVersion.OriginalVersion)] = av.OriginalVersion + 1;
                }
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
