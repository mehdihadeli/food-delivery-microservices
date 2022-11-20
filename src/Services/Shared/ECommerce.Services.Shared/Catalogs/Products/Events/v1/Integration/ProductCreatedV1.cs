
using BuildingBlocks.Core.Messaging;

namespace ECommerce.Services.Shared.Catalogs.Products.Events.v1.Integration;

public record ProductCreatedV1(long Id, string Name, long CategoryId, string CategoryName, int Stock) :
    IntegrationEvent;
