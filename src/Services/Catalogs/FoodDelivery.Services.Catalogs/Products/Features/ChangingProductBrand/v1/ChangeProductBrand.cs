using BuildingBlocks.Abstractions.Commands;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingProductBrand.v1;

public record ChangeProductBrand : ITxCommand<ChangeProductBrandResult>;

public record ChangeProductBrandResult;

public class ChangeProductBrandHandler : ICommandHandler<ChangeProductBrand, ChangeProductBrandResult>
{
    public ValueTask<ChangeProductBrandResult> Handle(ChangeProductBrand request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<ChangeProductBrandResult>(null!);
    }
}
