using BuildingBlocks.Core.Messaging;

namespace ECommerce.Services.Shared.Catalogs.Suppliers.Events.v1.Integration;

public record SupplierCreatedV1(long Id, string Name) : IntegrationEvent;
