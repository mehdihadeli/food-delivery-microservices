using BuildingBlocks.Core.CQRS.Events.Internal;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingMaxThreshold;

public record MaxThresholdChanged(long ProductId, int MaxThreshold) : DomainEvent;
