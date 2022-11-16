using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.Caching;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Core.Exception;
using ECommerce.Services.Catalogs.Products.Dtos;
using ECommerce.Services.Catalogs.Products.Exceptions.Application;
using ECommerce.Services.Catalogs.Shared.Contracts;
using ECommerce.Services.Catalogs.Shared.Extensions;

namespace ECommerce.Services.Catalogs.Products.Features.GettingProductById;

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

    internal class Cache : ICacheRequest<GetProductById, GetProductByIdResponse>
    {
        // Optionally, change defaults
        public TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromMinutes(10);
        public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(1);
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

            var product = await _catalogDbContext.FindProductByIdAsync(query.Id);
            Guard.Against.NotFound(product, new ProductCustomNotFoundException(query.Id));

            var productsDto = _mapper.Map<ProductDto>(product);

            return new GetProductByIdResponse(productsDto);
        }
    }
}
