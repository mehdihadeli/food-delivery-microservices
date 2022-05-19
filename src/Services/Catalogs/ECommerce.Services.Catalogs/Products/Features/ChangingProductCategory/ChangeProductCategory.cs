using BuildingBlocks.Abstractions.CQRS.Commands;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingProductCategory;

internal record ChangeProductCategory : ITxCommand<ChangeProductCategoryResult>;

internal record ChangeProductCategoryResult;

internal class ChangeProductCategoryHandler :
    ICommandHandler<ChangeProductCategory, ChangeProductCategoryResult>
{
    public Task<ChangeProductCategoryResult> Handle(
        ChangeProductCategory command,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<ChangeProductCategoryResult>(null!);
    }
}
