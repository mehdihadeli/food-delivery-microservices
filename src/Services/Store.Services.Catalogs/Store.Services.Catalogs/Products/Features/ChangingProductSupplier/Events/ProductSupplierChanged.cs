using BuildingBlocks.Core.CQRS.Event.Internal;
using Store.Services.Catalogs.Products.ValueObjects;
using Store.Services.Catalogs.Suppliers;

namespace Store.Services.Catalogs.Products.Features.ChangingProductSupplier.Events;

public record ProductSupplierChanged(SupplierId SupplierId, ProductId ProductId) : DomainEvent;
