namespace Store.Services.Catalogs.Products.Models;

public class ProductView
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public long CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    public long SupplierId { get; set; }
    public string SupplierName { get; set; } = default!;
    public long BrandId { get; set; }
    public string BrandName { get; set; } = default!;
}
