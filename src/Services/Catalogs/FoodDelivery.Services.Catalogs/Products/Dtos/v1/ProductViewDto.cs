namespace FoodDelivery.Services.Catalogs.Products.Dtos.V1;

public record ProductViewDto(long Id, string Name, string CategoryName, string SupplierName, long ItemCount);
