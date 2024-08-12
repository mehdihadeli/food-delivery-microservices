using BuildingBlocks.Abstractions.Commands;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingRestockThreshold.V1;

public record ChangeRestockThreshold(long ProductId, int NewRestockThreshold) : ITxCommand;
