using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Caching;
using BuildingBlocks.Core.Exception;
using ECommerce.Services.Catalogs.Products.Dtos.v1;
using ECommerce.Services.Catalogs.Products.Exceptions.Application;
using ECommerce.Services.Catalogs.Products.ValueObjects;
using ECommerce.Services.Catalogs.Shared.Contracts;
using ECommerce.Services.Catalogs.Shared.Extensions;
using FluentValidation;

namespace ECommerce.Services.Catalogs.Products.Features.GettingProductById.v1;

public record GetProductById(long Id) : IQuery<GetProductByIdResponse>
{
    internal class Validator : AbstractValidator<GetProductById>
    {
        public Validator()
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    public class Cache : CacheRequest<GetProductById, GetProductByIdResponse>
    {
        public override string CacheKey(GetProductById request)
        {
            return $"{base.CacheKey(request)}_{request.Id}";
        }
    }

    internal class Handler : IQueryHandler<GetProductById, GetProductByIdResponse>
    {
        private readonly ICatalogDbContext _catalogDbContext;
        private readonly IMapper _mapper;

        public Handler(ICatalogDbContext catalogDbContext, IMapper mapper)
        {
            _catalogDbContext = catalogDbContext;
            _mapper = mapper;
        }

        public async Task<GetProductByIdResponse> Handle(GetProductById query, CancellationToken cancellationToken)
        {
            Guard.Against.Null(query, nameof(query));

            var product = await _catalogDbContext.FindProductByIdAsync(ProductId.Of(query.Id));
            Guard.Against.NotFound(product, new ProductNotFoundException(query.Id));

            var productsDto = _mapper.Map<ProductDto>(product);

            return new GetProductByIdResponse(productsDto);
        }
    }
}
