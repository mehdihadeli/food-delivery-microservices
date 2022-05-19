using BuildingBlocks.Core.CQRS.Events.Internal;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingRestockThreshold;

public record RestockThresholdChanged(long ProductId, int RestockThreshold) : DomainEvent;
