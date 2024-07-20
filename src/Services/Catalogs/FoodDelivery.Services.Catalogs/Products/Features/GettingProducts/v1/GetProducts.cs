using AutoMapper;
using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Core.CQRS.Queries;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Paging;
using BuildingBlocks.Validation.Extensions;
using FoodDelivery.Services.Catalogs.Products.Dtos.v1;
using FoodDelivery.Services.Catalogs.Products.Models;
using FoodDelivery.Services.Catalogs.Products.Models.Read;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using FluentValidation;
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

internal class GetProductsHandler : IQueryHandler<GetProducts, GetProductsResult>
{
    private readonly ICatalogDbContext _catalogDbContext;
    private readonly ISieveProcessor _sieveProcessor;
    private readonly IMapper _mapper;

    public GetProductsHandler(IMapper mapper, ICatalogDbContext catalogDbContext, ISieveProcessor sieveProcessor)
    {
        _catalogDbContext = catalogDbContext;
        _sieveProcessor = sieveProcessor;
        _mapper = mapper;
    }

    public async Task<GetProductsResult> Handle(GetProducts request, CancellationToken cancellationToken)
    {
        var products = await _catalogDbContext.Products
            .OrderByDescending(x => x.Created)
            .AsNoTracking()
            .ApplyPagingAsync<Product, ProductReadModel>(
                request,
                _mapper.ConfigurationProvider,
                _sieveProcessor,
                cancellationToken: cancellationToken
            );

        var result = products.MapTo<ProductDto>(_mapper);

        return new GetProductsResult(result);
    }
}

internal record GetProductsResult(IPageList<ProductDto> Products);
