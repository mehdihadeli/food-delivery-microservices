namespace FoodDelivery.Services.Catalogs.Categories.Contracts;

public interface ICategoryChecker
{
    bool CategoryExists(CategoryId categoryId);
}
