using BuildingBlocks.Core.Messaging;

namespace FoodDelivery.Services.Shared.Catalogs.Suppliers.Events.v1.Integration;

public record SupplierDeletedV1(long Id) : IntegrationEvent;
