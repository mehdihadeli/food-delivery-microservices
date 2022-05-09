using Bogus;
using BuildingBlocks.Abstractions.Persistence;
using Store.Services.Catalogs.Shared.Contracts;

namespace Store.Services.Catalogs.Brands.Data;

public class BrandDataSeeder : IDataSeeder
{
    private readonly ICatalogDbContext _context;

    public BrandDataSeeder(ICatalogDbContext context)
    {
        _context = context;
    }

    public async Task SeedAllAsync()
    {
        if (await _context.Brands.AnyAsync())
            return;

        long id = 1;

        // https://github.com/bchavez/Bogus
        // https://www.youtube.com/watch?v=T9pwE1GAr_U
        var brandFaker = new Faker<Brand>().CustomInstantiator(faker =>
        {
            var brand = Brand.Create(id, faker.Company.CompanyName());
            id++;
            return brand;
        });
        var brands = brandFaker.Generate(5);

        await _context.Brands.AddRangeAsync(brands);
        await _context.SaveChangesAsync();
    }
}
