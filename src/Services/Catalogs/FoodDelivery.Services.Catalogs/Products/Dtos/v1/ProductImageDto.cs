namespace FoodDelivery.Services.Catalogs.Products.Dtos.V1;

public record ProductImageDto(long Id, long ProductId, string ImageUrl, bool IsMain = false);
