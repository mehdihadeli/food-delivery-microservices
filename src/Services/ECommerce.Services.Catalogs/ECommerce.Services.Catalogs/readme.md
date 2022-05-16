#### Migration Scripts

```bash
dotnet ef migrations add InitialCatalogMigration -o Shared\Data\Migrations\Catalogs -c CatalogDbContext
dotnet ef database update -c CatalogDbContext

dotnet ef migrations bundle -o Shared\Data\Migrations\Catalogs\Bundle -c CatalogDbContext
```
