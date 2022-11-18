using BuildingBlocks.Core.CQRS.Queries;
using ECommerce.Services.Customers.RestockSubscriptions.Dtos;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptions.v1;

public record GetRestockSubscriptionsResponse(ListResultModel<RestockSubscriptionDto> RestockSubscriptions);
