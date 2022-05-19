using BuildingBlocks.Core.CQRS.Events.Internal;
using ECommerce.Services.Catalogs.Products.ValueObjects;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingProductPrice;

public record ProductPriceChanged(Price Price) : DomainEvent;
