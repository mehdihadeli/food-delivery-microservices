
IF "%1"=="init-context" dotnet ef migrations add InitialCatalogMigration -o \ECommerce.Services.Catalogs\Shared\Data\Migrations\Catalogs --project .\ECommerce.Services.Catalogs\ECommerce.Services.Catalogs.csproj -c CatalogDbContext --verbose & goto exit
IF "%1"=="update-context" dotnet ef database update -c CatalogDbContext --verbose --project .\ECommerce.Services.Catalogs\ECommerce.Services.Catalogs.csproj & goto exit 

:exit