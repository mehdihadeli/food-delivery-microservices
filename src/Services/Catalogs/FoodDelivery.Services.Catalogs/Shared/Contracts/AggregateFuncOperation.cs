namespace FoodDelivery.Services.Catalogs.Shared.Contracts;

public delegate Task<TResult> AggregateFuncOperation<in T, TResult>(T input);

public delegate Task AggregateActionOperation<in T>(T input);
