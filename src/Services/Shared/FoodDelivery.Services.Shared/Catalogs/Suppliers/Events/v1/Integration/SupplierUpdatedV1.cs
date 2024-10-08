using BuildingBlocks.Core.Messaging;

namespace FoodDelivery.Services.Shared.Catalogs.Suppliers.Events.V1.Integration;

public record SupplierUpdatedV1(long Id, string Name) : IntegrationEvent;
