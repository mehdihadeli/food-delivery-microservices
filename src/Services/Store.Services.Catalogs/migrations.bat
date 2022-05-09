
IF "%1"=="init-context" dotnet ef migrations add InitialCatalogMigration -o \Store.Services.Catalogs\Shared\Data\Migrations\Catalogs --project .\Store.Services.Catalogs\Store.Services.Catalogs.csproj -c CatalogDbContext --verbose & goto exit
IF "%1"=="update-context" dotnet ef database update -c CatalogDbContext --verbose --project .\Store.Services.Catalogs\Store.Services.Catalogs.csproj & goto exit 

:exit