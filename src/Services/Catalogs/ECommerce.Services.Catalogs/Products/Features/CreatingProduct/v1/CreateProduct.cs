using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Core.Exception;
using BuildingBlocks.Core.IdsGenerator;
using ECommerce.Services.Catalogs.Brands;
using ECommerce.Services.Catalogs.Brands.Exceptions.Application;
using ECommerce.Services.Catalogs.Categories;
using ECommerce.Services.Catalogs.Categories.Exceptions.Domain;
using ECommerce.Services.Catalogs.Products.Dtos.v1;
using ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1.Requests;
using ECommerce.Services.Catalogs.Products.Models;
using ECommerce.Services.Catalogs.Products.ValueObjects;
using ECommerce.Services.Catalogs.Shared.Contracts;
using ECommerce.Services.Catalogs.Shared.Extensions;
using ECommerce.Services.Catalogs.Suppliers;
using ECommerce.Services.Catalogs.Suppliers.Exceptions.Application;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1;

public record CreateProduct(
    string Name,
    decimal Price,
    int Stock,
    int RestockThreshold,
    int MaxStockThreshold,
    ProductStatus Status,
    int Width,
    int Height,
    int Depth,
    string Size,
    ProductColor Color,
    long CategoryId,
    long SupplierId,
    long BrandId,
    string? Description = null,
    IEnumerable<CreateProductImageRequest>? Images = null) : ITxCreateCommand<CreateProductResponse>
{
    public long Id { get; init; } = SnowFlakIdGenerator.NewId();
}

public class CreateProductValidator : AbstractValidator<CreateProduct>
{
    public CreateProductValidator()
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Id)
            .NotEmpty()
            .GreaterThan(0).WithMessage("InternalCommandId must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Price)
            .NotEmpty()
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status is required.");

        RuleFor(x => x.Color)
            .IsInEnum().WithMessage("Color is required.");

        RuleFor(x => x.Stock)
            .NotEmpty()
            .GreaterThan(0).WithMessage("Stock must be greater than 0");

        RuleFor(x => x.MaxStockThreshold)
            .NotEmpty()
            .GreaterThan(0).WithMessage("MaxStockThreshold must be greater than 0");

        RuleFor(x => x.RestockThreshold)
            .NotEmpty()
            .GreaterThan(0).WithMessage("RestockThreshold must be greater than 0");

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .GreaterThan(0).WithMessage("CategoryId must be greater than 0");

        RuleFor(x => x.SupplierId)
            .NotEmpty()
            .GreaterThan(0).WithMessage("SupplierId must be greater than 0");

        RuleFor(x => x.BrandId)
            .NotEmpty()
            .GreaterThan(0).WithMessage("BrandId must be greater than 0");
    }
}

public class CreateProductHandler : ICommandHandler<CreateProduct, CreateProductResponse>
{
    private readonly ILogger<CreateProductHandler> _logger;
    private readonly IMapper _mapper;
    private readonly ICatalogDbContext _catalogDbContext;

    public CreateProductHandler(
        ICatalogDbContext catalogDbContext,
        IMapper mapper,
        ILogger<CreateProductHandler> logger)
    {
        _logger = Guard.Against.Null(logger, nameof(logger));
        _mapper = Guard.Against.Null(mapper, nameof(mapper));
        _catalogDbContext = Guard.Against.Null(catalogDbContext, nameof(catalogDbContext));
    }

    public async Task<CreateProductResponse> Handle(
        CreateProduct command,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(command, nameof(command));

        var images = command.Images?.Select(x =>
                new ProductImage(
                    EntityId.CreateEntityId(SnowFlakIdGenerator.NewId()),
                    x.ImageUrl,
                    x.IsMain,
                    ProductId.Of(command.Id)))
            .ToList();

        var category = await _catalogDbContext.FindCategoryAsync(CategoryId.Of(command.CategoryId));
        Guard.Against.NotFound(category, new CategoryDomainException(command.CategoryId));

        var brand = await _catalogDbContext.FindBrandAsync(BrandId.Of(command.BrandId));
        Guard.Against.NotFound(brand, new BrandNotFoundException(command.BrandId));

        var supplier = await _catalogDbContext.FindSupplierByIdAsync(SupplierId.Of(command.SupplierId));
        Guard.Against.NotFound(supplier, new SupplierNotFoundException(command.SupplierId));

        // await _domainEventDispatcher.DispatchAsync(cancellationToken, new Events.Domain.CreatingProduct());
        var product = Product.Create(
            ProductId.Of(command.Id),
            Name.Of(command.Name),
            Stock.Of(command.Stock, command.RestockThreshold, command.MaxStockThreshold),
            command.Status,
            Dimensions.Of(command.Width, command.Height, command.Depth),
            Size.Of(command.Size),
            command.Color,
            command.Description,
            Price.Of(command.Price),
            category!.Id,
            supplier!.Id,
            brand!.Id,
            images);

        await _catalogDbContext.Products.AddAsync(product, cancellationToken: cancellationToken);

        await _catalogDbContext.SaveChangesAsync(cancellationToken);

        var created = await _catalogDbContext.Products
            .Include(x => x.Brand)
            .Include(x => x.Category)
            .Include(x => x.Supplier)
            .SingleOrDefaultAsync(x => x.Id == product.Id, cancellationToken: cancellationToken);

        var productDto = _mapper.Map<ProductDto>(created);

        _logger.LogInformation("Product a with ID: '{ProductId} created.'", command.Id);

        return new CreateProductResponse(productDto);
    }
}
