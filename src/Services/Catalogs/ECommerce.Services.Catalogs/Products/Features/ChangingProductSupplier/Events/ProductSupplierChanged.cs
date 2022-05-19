using BuildingBlocks.Core.CQRS.Events.Internal;
using ECommerce.Services.Catalogs.Products.ValueObjects;
using ECommerce.Services.Catalogs.Suppliers;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingProductSupplier.Events;

public record ProductSupplierChanged(SupplierId SupplierId, ProductId ProductId) : DomainEvent;
