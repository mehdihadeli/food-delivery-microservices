using AutoMapper;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Paging;
using BuildingBlocks.Core.Queries;
using BuildingBlocks.Validation.Extensions;
using Dapper;
using FluentValidation;
using FoodDelivery.Services.Catalogs.Products.Dtos.v1;
using FoodDelivery.Services.Catalogs.Products.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Catalogs.Products.Features.GettingProductsView.v1;

internal record GetProductsView : PageQuery<GetProductsViewResult>
{
    public static GetProductsView Of(PageRequest pageRequest)
    {
        var (pageNumber, pageSize, filters, sortOrder) = pageRequest;

        return new GetProductsViewValidator().HandleValidation(
            new GetProductsView
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Filters = filters,
                SortOrder = sortOrder
            }
        );
    }
}

internal class GetProductsViewValidator : AbstractValidator<GetProductsView>
{
    public GetProductsViewValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page should at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageSize should at least greater than or equal to 1.");
    }
}

internal class GetProductsViewHandler(IDbFacadeResolver facadeResolver, IMapper mapper)
    : IRequestHandler<GetProductsView, GetProductsViewResult>
{
    public async Task<GetProductsViewResult> Handle(GetProductsView request, CancellationToken cancellationToken)
    {
        await using var conn = facadeResolver.Database.GetDbConnection();
        var (pageNumber, pageSize, filters, sortOrder) = request;
        await conn.OpenAsync(cancellationToken);
        var results = await conn.QueryAsync<ProductView>(
            @"SELECT product_id ""InternalCommandId"", product_name ""Name"", category_name CategoryName, supplier_name SupplierName, count(*) OVER() AS ItemCount
                    FROM catalog.product_views LIMIT @PageSize OFFSET ((@Page - 1) * @PageSize)",
            new { pageSize, pageNumber }
        );

        var productViewDtos = mapper.Map<IEnumerable<ProductViewDto>>(results);

        return new GetProductsViewResult(productViewDtos);
    }
}

internal record GetProductsViewResult(IEnumerable<ProductViewDto> Products);
