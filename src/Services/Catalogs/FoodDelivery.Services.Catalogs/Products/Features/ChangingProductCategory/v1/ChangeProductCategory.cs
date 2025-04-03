using BuildingBlocks.Abstractions.Commands;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingProductCategory.v1;

public record ChangeProductCategory : ITxCommand<ChangeProductCategoryResult>;

public record ChangeProductCategoryResult;

public class ChangeProductCategoryHandler : ICommandHandler<ChangeProductCategory, ChangeProductCategoryResult>
{
    public ValueTask<ChangeProductCategoryResult> Handle(
        ChangeProductCategory request,
        CancellationToken cancellationToken
    )
    {
        return ValueTask.FromResult<ChangeProductCategoryResult>(null!);
    }
}
