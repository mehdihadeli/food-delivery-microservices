using BuildingBlocks.Abstractions.Commands;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingProductBrand.V1;

internal record ChangeProductBrand : ITxCommand<ChangeProductBrandResult>;

internal record ChangeProductBrandResult;

internal class ChangeProductBrandHandler : ICommandHandler<ChangeProductBrand, ChangeProductBrandResult>
{
    public Task<ChangeProductBrandResult> Handle(ChangeProductBrand command, CancellationToken cancellationToken)
    {
        return Task.FromResult<ChangeProductBrandResult>(null!);
    }
}
