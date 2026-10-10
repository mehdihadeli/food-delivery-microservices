using FoodDelivery.Services.Catalogs.Products;
using FoodDelivery.Services.Catalogs.Products.Models;
using FoodDelivery.Services.Catalogs.Products.Models.Read;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Catalogs.UnitTests.Products;

public class ProductsMappingTests
{
    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_map_product_read_model_to_product_dto()
    {
        var productReadModel = new ProductReadModel
        {
            Id = 1,
            Name = "Pizza",
            Description = "Stone-baked pizza",
            Price = 12.50m,
            CategoryId = 2,
            CategoryName = "Main course",
            SupplierId = 3,
            SupplierName = "Fresh Foods",
            BrandId = 4,
            BrandName = "House brand",
            AvailableStock = 10,
            RestockThreshold = 2,
            MaxStockThreshold = 20,
            ProductStatus = ProductStatus.Available,
            ProductColor = ProductColor.Red,
            Size = "Large",
            Height = 3,
            Width = 30,
            Depth = 30,
        };

        var productDto = productReadModel.ToProductDto();

        productDto.Id.ShouldBe(productReadModel.Id);
        productDto.Name.ShouldBe(productReadModel.Name);
        productDto.Description.ShouldBe(productReadModel.Description);
        productDto.Price.ShouldBe(productReadModel.Price);
        productDto.CategoryId.ShouldBe(productReadModel.CategoryId);
        productDto.CategoryName.ShouldBe(productReadModel.CategoryName);
        productDto.SupplierId.ShouldBe(productReadModel.SupplierId);
        productDto.SupplierName.ShouldBe(productReadModel.SupplierName);
        productDto.BrandId.ShouldBe(productReadModel.BrandId);
        productDto.BrandName.ShouldBe(productReadModel.BrandName);
        productDto.AvailableStock.ShouldBe(productReadModel.AvailableStock);
        productDto.RestockThreshold.ShouldBe(productReadModel.RestockThreshold);
        productDto.MaxStockThreshold.ShouldBe(productReadModel.MaxStockThreshold);
        productDto.ProductStatus.ShouldBe(productReadModel.ProductStatus);
        productDto.ProductColor.ShouldBe(productReadModel.ProductColor);
        productDto.Size.ShouldBe(productReadModel.Size);
        productDto.Height.ShouldBe(productReadModel.Height);
        productDto.Width.ShouldBe(productReadModel.Width);
        productDto.Depth.ShouldBe(productReadModel.Depth);
    }
}
