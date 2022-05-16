
IF "%1"=="init-context" dotnet ef migrations add InitialIdentityServerMigration -o \ECommerce.Services.Identity\Shared\Data\Migrations\Identity --project .\ECommerce.Services.Identity\ECommerce.Services.Identity.csproj -c IdentityContext --verbose & goto exit
IF "%1"=="update-context" dotnet ef database update -c IdentityContext --verbose --project .\ECommerce.Services.Identity\ECommerce.Services.Identity.csproj & goto exit 
:exit