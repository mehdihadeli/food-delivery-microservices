using BuildingBlocks.Core.Messaging;

namespace ECommerce.Services.Shared.Catalogs.Suppliers.Events.v1.Integration;

public record SupplierUpdated(long Id, string Name) : IntegrationEvent;
