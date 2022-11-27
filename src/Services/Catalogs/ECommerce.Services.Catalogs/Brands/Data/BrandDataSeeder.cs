using AutoBogus;
using Bogus;
using BuildingBlocks.Abstractions.Persistence;
using ECommerce.Services.Catalogs.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Catalogs.Brands.Data;

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

        // https://www.youtube.com/watch?v=T9pwE1GAr_U
        // https://jackhiston.com/2017/10/1/how-to-create-bogus-data-in-c/
        // https://khalidabuhakmeh.com/seed-entity-framework-core-with-bogus
        // https://github.com/bchavez/Bogus#bogus-api-support
        // https://github.com/bchavez/Bogus/blob/master/Examples/EFCoreSeedDb/Program.cs#L74
        long id = 1;

        // faker works with normal syntax because brand has a default constructor
        var brands = new AutoFaker<Brand>()
            .RuleFor(m => m.Id, f => new BrandId(id++))
            .RuleFor(m => m.Name, f => f.Company.CompanyName())
            .FinishWith((f, u) =>
            {
                Console.WriteLine("Brand Created! Id={0}", u.Id);
            });

        await _context.Brands.AddRangeAsync(brands.Generate(5));
        await _context.SaveChangesAsync();
    }

    public int Order => 3;
}
