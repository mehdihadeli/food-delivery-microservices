using BuildingBlocks.Abstractions.CQRS.Commands;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingRestockThreshold;

public record ChangeRestockThreshold(long ProductId, int NewRestockThreshold) : ITxCommand;
