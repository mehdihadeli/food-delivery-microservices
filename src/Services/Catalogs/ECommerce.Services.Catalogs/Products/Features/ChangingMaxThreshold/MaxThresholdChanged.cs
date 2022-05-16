using BuildingBlocks.Core.CQRS.Event.Internal;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingMaxThreshold;

public record MaxThresholdChanged(long ProductId, int MaxThreshold) : DomainEvent;
