using BuildingBlocks.Core.CQRS.Events.Internal;
using ECommerce.Services.Catalogs.Products.ValueObjects;
using ECommerce.Services.Catalogs.Suppliers;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingProductSupplier.v1.Events;

public record ProductSupplierChanged(SupplierId SupplierId, ProductId ProductId) : DomainEvent;
