using BuildingBlocks.Abstractions.CQRS.Commands;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingMaxThreshold;

public record ChangeMaxThreshold(long ProductId, int NewMaxThreshold) : ITxCommand;
