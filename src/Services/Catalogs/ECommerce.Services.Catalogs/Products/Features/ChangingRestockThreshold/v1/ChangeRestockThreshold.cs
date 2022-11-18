using BuildingBlocks.Abstractions.CQRS.Commands;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingRestockThreshold.v1;

public record ChangeRestockThreshold(long ProductId, int NewRestockThreshold) : ITxCommand;
