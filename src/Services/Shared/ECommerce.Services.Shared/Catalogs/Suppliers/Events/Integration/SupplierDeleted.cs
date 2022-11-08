using BuildingBlocks.Core.Messaging;

namespace ECommerce.Services.Shared.Catalogs.Suppliers.Events.Integration;

public record SupplierDeleted(long Id) : IntegrationEvent;
