using BuildingBlocks.Core.CQRS.Queries;
using ECommerce.Services.Customers.RestockSubscriptions.Dtos;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptions;

public record GetRestockSubscriptionsResponse(ListResultModel<RestockSubscriptionDto> RestockSubscriptions);
