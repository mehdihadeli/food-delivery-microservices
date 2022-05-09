
IF "%1"=="init-context" dotnet ef migrations add InitialCustomersMigration -o \Store.Services.Customer\Shared\Data\Migrations\Customer --project .\Store.Services.Customers\Store.Services.Customers.csproj -c CustomersDbContext --verbose & goto exit
IF "%1"=="update-context" dotnet ef database update -c CustomersDbContext --verbose --project .\Store.Services.Customers\Store.Services.Customers.csproj & goto exit 

:exit