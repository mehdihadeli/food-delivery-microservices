
IF "%1"=="init-context" dotnet ef migrations add InitialCatalogMigration -o \FoodDelivery.Services.Catalogs\Shared\Data\Migrations\Catalogs --project .\FoodDelivery.Services.Catalogs\FoodDelivery.Services.Catalogs.csproj -c CatalogDbContext --verbose & goto exit
IF "%1"=="update-context" dotnet ef database update -c CatalogDbContext --verbose --project .\FoodDelivery.Services.Catalogs\FoodDelivery.Services.Catalogs.csproj & goto exit 

:exit