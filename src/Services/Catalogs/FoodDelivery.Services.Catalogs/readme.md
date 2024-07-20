#### Migration Scripts

```bash
dotnet ef migrations add InitialCatalogMigration -o Shared/Data/Migrations/Catalogs -c CatalogDbContext
dotnet ef database update -c CatalogDbContext
```
