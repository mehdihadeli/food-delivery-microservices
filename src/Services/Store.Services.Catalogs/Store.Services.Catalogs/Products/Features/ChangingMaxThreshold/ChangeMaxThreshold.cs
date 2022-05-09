using BuildingBlocks.Abstractions.CQRS.Command;

namespace Store.Services.Catalogs.Products.Features.ChangingMaxThreshold;

public record ChangeMaxThreshold(long ProductId, int NewMaxThreshold) : ITxCommand;
