namespace ECommerce.Services.Catalogs.Products.Features.GettingProductsView;

public class GetProductsViewRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
