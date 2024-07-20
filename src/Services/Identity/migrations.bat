
IF "%1"=="init-context" dotnet ef migrations add InitialIdentityServerMigration -o \FoodDelivery.Services.Identity\Shared\Data\Migrations\Identity --project .\FoodDelivery.Services.Identity\FoodDelivery.Services.Identity.csproj -c IdentityContext --verbose & goto exit
IF "%1"=="update-context" dotnet ef database update -c IdentityContext --verbose --project .\FoodDelivery.Services.Identity\FoodDelivery.Services.Identity.csproj & goto exit 
:exit