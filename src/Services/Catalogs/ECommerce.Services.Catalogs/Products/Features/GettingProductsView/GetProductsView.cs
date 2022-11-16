using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Abstractions.Persistence;
using Dapper;
using ECommerce.Services.Catalogs.Products.Models;

namespace ECommerce.Services.Catalogs.Products.Features.GettingProductsView;

public record GetProductsView : IQuery<GetProductsViewResponse>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    internal class Validator : AbstractValidator<GetProductsView>
    {
        public Validator()
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1).WithMessage("Page should at least greater than or equal to 1.");

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1).WithMessage("PageSize should at least greater than or equal to 1.");
        }
    }

    internal class Handler : IRequestHandler<GetProductsView, GetProductsViewResponse>
    {
        private readonly IDbFacadeResolver _facadeResolver;
        private readonly IMapper _mapper;

        public Handler(IDbFacadeResolver facadeResolver, IMapper mapper)
        {
            _facadeResolver = facadeResolver;
            _mapper = mapper;
        }

        public async Task<GetProductsViewResponse> Handle(
            GetProductsView request,
            CancellationToken cancellationToken)
        {
            await using var conn = _facadeResolver.Database.GetDbConnection();
            await conn.OpenAsync(cancellationToken);
            var results = await conn.QueryAsync<ProductView>(
                @"SELECT product_id ""Id"", product_name ""Name"", category_name CategoryName, supplier_name SupplierName, count(*) OVER() AS ItemCount
                    FROM catalog.product_views LIMIT @PageSize OFFSET ((@Page - 1) * @PageSize)",
                new {request.PageSize, request.Page}
            );

            var productViewDtos = _mapper.Map<IEnumerable<ProductViewDto>>(results);

            return new GetProductsViewResponse(productViewDtos);
        }
    }
}
