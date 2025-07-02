namespace FoodDelivery.BlazorWebApp.Dtos;

using System.Collections.Generic;

public class ProductDto
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int RestockThreshold { get; set; }
    public int MaxStockThreshold { get; set; }
    public int Status { get; set; }
    public int ProductType { get; set; }
    public int ProductColor { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
    public int Depth { get; set; }
    public string Size { get; set; } = default!;
    public long CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    public long SupplierId { get; set; }
    public string SupplierName { get; set; } = default!;
    public long BrandId { get; set; }
    public string BrandName { get; set; } = default!;
    public string? Description { get; set; }
    public IList<ProductImageDto>? Images { get; set; } = new List<ProductImageDto>();
}

public class ProductImageDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string ImageUrl { get; set; } = default!;
    public bool IsMain { get; set; }
}
