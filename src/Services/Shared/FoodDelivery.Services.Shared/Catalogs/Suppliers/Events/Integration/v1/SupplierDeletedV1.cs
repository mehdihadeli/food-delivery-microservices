using BuildingBlocks.Core.Messages;

namespace FoodDelivery.Services.Shared.Catalogs.Suppliers.Events.Integration.v1;

public record SupplierDeletedV1(long Id) : IntegrationEvent;
