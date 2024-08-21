using AutoMapper;
using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Paging;
using BuildingBlocks.Core.Queries;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Catalogs.Products.Dtos.v1;
using FoodDelivery.Services.Catalogs.Products.Models;
using FoodDelivery.Services.Catalogs.Products.Models.Read;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using Microsoft.EntityFrameworkCore;
using Sieve.Services;

namespace FoodDelivery.Services.Catalogs.Products.Features.GettingProducts.v1;

internal record GetProducts : PageQuery<GetProductsResult>
{
    /// <summary>
    /// Get Products with in-line validation.
    /// </summary>
    /// <param name="pageRequest"></param>
    /// <returns></returns>
    public static GetProducts Of(PageRequest pageRequest)
    {
        var (pageNumber, pageSize, filters, sortOrder) = pageRequest;

        return new GetProductsValidator().HandleValidation(
            new GetProducts
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Filters = filters,
                SortOrder = sortOrder
            }
        );
    }
}

internal class GetProductsValidator : AbstractValidator<GetProducts>
{
    public GetProductsValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page should at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageSize should at least greater than or equal to 1.");
    }
}

internal class GetProductsHandler(IMapper mapper, ICatalogDbContext catalogDbContext, ISieveProcessor sieveProcessor)
    : IQueryHandler<GetProducts, GetProductsResult>
{
    public async Task<GetProductsResult> Handle(GetProducts request, CancellationToken cancellationToken)
    {
        var products = await catalogDbContext
            .Products.OrderByDescending(x => x.Created)
            .AsNoTracking()
            .ApplyPagingAsync<Product, ProductReadModel>(
                request,
                sieveProcessor,
                mapper.ConfigurationProvider,
                cancellationToken: cancellationToken
            );

        var result = products.MapTo<ProductDto>(mapper);

        return new GetProductsResult(result);
    }
}

internal record GetProductsResult(IPageList<ProductDto> Products);
