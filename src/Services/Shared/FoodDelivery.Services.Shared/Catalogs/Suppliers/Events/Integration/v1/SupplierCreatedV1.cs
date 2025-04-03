using BuildingBlocks.Core.Messages;

namespace FoodDelivery.Services.Shared.Catalogs.Suppliers.Events.Integration.v1;

public record SupplierCreatedV1(long Id, string Name) : IntegrationEvent;
