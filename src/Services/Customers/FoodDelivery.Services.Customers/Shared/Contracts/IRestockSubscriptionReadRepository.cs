using FoodDelivery.Services.Customers.RestockSubscriptions.Models.Read;
using Squirrel.Abstractions.Persistence;

namespace FoodDelivery.Services.Customers.Shared.Contracts;

public interface IRestockSubscriptionReadRepository : IRepository<RestockSubscriptionReadModel, Guid> { }
