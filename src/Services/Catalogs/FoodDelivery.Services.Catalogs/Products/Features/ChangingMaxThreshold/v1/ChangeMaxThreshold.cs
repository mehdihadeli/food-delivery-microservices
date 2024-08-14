using BuildingBlocks.Abstractions.Commands;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingMaxThreshold.v1;

public record ChangeMaxThreshold(long ProductId, int NewMaxThreshold) : ITxCommand;
