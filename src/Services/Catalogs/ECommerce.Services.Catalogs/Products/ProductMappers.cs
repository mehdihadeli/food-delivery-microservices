using AutoMapper;
using ECommerce.Services.Catalogs.Products.Dtos;
using ECommerce.Services.Catalogs.Products.Dtos.v1;
using ECommerce.Services.Catalogs.Products.Features.CreatingProduct;
using ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1;
using ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1.Requests;
using ECommerce.Services.Catalogs.Products.Features.GettingProductsView;
using ECommerce.Services.Catalogs.Products.Features.GettingProductsView.v1;
using ECommerce.Services.Catalogs.Products.Models;

namespace ECommerce.Services.Catalogs.Products;

public class ProductMappers : Profile
{
    public ProductMappers()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(x => x.Depth, opt => opt.MapFrom(x => x.Dimensions.Depth))
            .ForMember(x => x.Height, opt => opt.MapFrom(x => x.Dimensions.Height))
            .ForMember(x => x.Width, opt => opt.MapFrom(x => x.Dimensions.Width))
            .ForMember(x => x.AvailableStock, opt => opt.MapFrom(x => x.Stock.Available))
            .ForMember(x => x.RestockThreshold, opt => opt.MapFrom(x => x.Stock.RestockThreshold))
            .ForMember(x => x.MaxStockThreshold, opt => opt.MapFrom(x => x.Stock.MaxStockThreshold))
            .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Name.Value))
            .ForMember(x => x.Price, opt => opt.MapFrom(x => x.Price.Value))
            .ForMember(x => x.BrandId, opt => opt.MapFrom(x => x.BrandId.Value))
            .ForMember(x => x.BrandName, opt => opt.MapFrom(x => x.Brand == null ? "" : x.Brand.Name))
            .ForMember(x => x.CategoryName, opt => opt.MapFrom(x => x.Category == null ? "" : x.Category.Name))
            .ForMember(x => x.CategoryId, opt => opt.MapFrom(x => x.CategoryId.Value))
            .ForMember(x => x.SupplierName, opt => opt.MapFrom(x => x.Supplier == null ? "" : x.Supplier.Name))
            .ForMember(x => x.SupplierId, opt => opt.MapFrom(x => x.SupplierId.Value))
            .ForMember(x => x.Id, opt => opt.MapFrom(x => x.Id.Value))
            .ForMember(x => x.ProductStatus, opt => opt.MapFrom(x => x.ProductStatus))
            .ForMember(x => x.Size, opt => opt.MapFrom(x => x.Size.Value))
            .ForMember(x => x.ProductColor, opt => opt.MapFrom(x => x.Color))
            .ForMember(x => x.Description, opt => opt.MapFrom(x => x.Description))
            .ForMember(x => x.Images, opt => opt.MapFrom(x => x.Images));

        CreateMap<ProductImage, ProductImageDto>()
            .ForMember(x => x.Id, opt => opt.MapFrom(x => x.Id.Value))
            .ForMember(x => x.ProductId, opt => opt.MapFrom(x => x.ProductId.Value));

        CreateMap<ProductView, ProductViewDto>();

        CreateMap<CreateProduct, Product>();

        CreateMap<CreateProductRequest, CreateProduct>()
            .ConstructUsing(req => new CreateProduct(
                req.Name,
                req.Price,
                req.Stock,
                req.RestockThreshold,
                req.MaxStockThreshold,
                req.Status,
                req.Width,
                req.Height,
                req.Depth,
                req.Size,
                req.Color,
                req.CategoryId,
                req.SupplierId,
                req.BrandId,
                req.Description,
                req.Images));
    }
}
