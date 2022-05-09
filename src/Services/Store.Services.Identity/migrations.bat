
IF "%1"=="init-context" dotnet ef migrations add InitialIdentityServerMigration -o \Store.Services.Identity\Shared\Data\Migrations\Identity --project .\Store.Services.Identity\Store.Services.Identity.csproj -c IdentityContext --verbose & goto exit
IF "%1"=="update-context" dotnet ef database update -c IdentityContext --verbose --project .\Store.Services.Identity\Store.Services.Identity.csproj & goto exit 
:exit