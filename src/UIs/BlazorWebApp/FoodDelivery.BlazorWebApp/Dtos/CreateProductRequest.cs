namespace FoodDelivery.BlazorWebApp.Dtos;

public class CreateProductRequest
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int RestockThreshold { get; set; }
    public int MaxStockThreshold { get; set; }
    public int Status { get; set; } // Enum value
    public int ProductType { get; set; } // Enum value
    public int Color { get; set; } // Enum value
    public int Height { get; set; }
    public int Width { get; set; }
    public int Depth { get; set; }
    public string Size { get; set; } = default!;
    public long CategoryId { get; set; }
    public long SupplierId { get; set; }
    public long BrandId { get; set; }
    public string? Description { get; set; }
    // public List<CreateProductImageRequest>? Images { get; set; }
}
