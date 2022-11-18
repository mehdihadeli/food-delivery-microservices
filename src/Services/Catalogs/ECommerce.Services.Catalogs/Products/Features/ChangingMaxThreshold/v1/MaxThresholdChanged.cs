using BuildingBlocks.Core.CQRS.Events.Internal;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingMaxThreshold.v1;

public record MaxThresholdChanged(long ProductId, int MaxThreshold) : DomainEvent;
