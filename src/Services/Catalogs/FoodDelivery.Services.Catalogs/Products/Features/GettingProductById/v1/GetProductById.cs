using AutoMapper;
using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Caching;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Catalogs.Products.Dtos.v1;
using FoodDelivery.Services.Catalogs.Products.Exceptions.Application;
using FoodDelivery.Services.Catalogs.Products.ValueObjects;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using FoodDelivery.Services.Catalogs.Shared.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Features.GettingProductById.v1;

internal record GetProductById(long Id) : CacheQuery<GetProductById, GetProductByIdResult>
{
    /// <summary>
    /// GetProductById query with validation.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static GetProductById Of(long id)
    {
        return new GetProductByIdValidator().HandleValidation(new GetProductById(id));
    }

    public override string CacheKey(GetProductById request)
    {
        return $"{base.CacheKey(request)}_{request.Id}";
    }
}

internal class GetProductByIdValidator : AbstractValidator<GetProductById>
{
    public GetProductByIdValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

internal class GetProductByIdHandler(ICatalogDbContext catalogDbContext, IMapper mapper)
    : IQueryHandler<GetProductById, GetProductByIdResult>
{
    public async Task<GetProductByIdResult> Handle(GetProductById query, CancellationToken cancellationToken)
    {
        query.NotBeNull();

        var product = await catalogDbContext.FindProductByIdAsync(ProductId.Of(query.Id));
        if (product is null)
            throw new ProductNotFoundException(query.Id);

        var productsDto = mapper.Map<ProductDto>(product);

        return new GetProductByIdResult(productsDto);
    }
}

internal record GetProductByIdResult(ProductDto Product);
