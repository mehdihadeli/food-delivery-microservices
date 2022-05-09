using BuildingBlocks.Core.CQRS.Event.Internal;
using Store.Services.Catalogs.Products.ValueObjects;

namespace Store.Services.Catalogs.Products.Features.ChangingProductPrice;

public record ProductPriceChanged(Price Price) : DomainEvent;
