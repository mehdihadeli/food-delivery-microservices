namespace FoodDelivery.BlazorWebApp.Dtos;

public class UpdateProductRequest
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int RestockThreshold { get; set; }
    public int MaxStockThreshold { get; set; }
    public int Status { get; set; }
    public int ProductType { get; set; }
    public int Color { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
    public int Depth { get; set; }
    public string Size { get; set; } = default!;
    public long CategoryId { get; set; }
    public long SupplierId { get; set; }
    public long BrandId { get; set; }
    public string? Description { get; set; }
}
