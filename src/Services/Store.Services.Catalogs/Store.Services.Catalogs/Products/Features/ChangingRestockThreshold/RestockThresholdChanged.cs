using BuildingBlocks.Core.CQRS.Event.Internal;

namespace Store.Services.Catalogs.Products.Features.ChangingRestockThreshold;

public record RestockThresholdChanged(long ProductId, int RestockThreshold) : DomainEvent;
