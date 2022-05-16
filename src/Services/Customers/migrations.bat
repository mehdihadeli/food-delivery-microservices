
IF "%1"=="init-context" dotnet ef migrations add InitialCustomersMigration -o \ECommerce.Services.Customer\Shared\Data\Migrations\Customer --project .\ECommerce.Services.Customers\ECommerce.Services.Customers.csproj -c CustomersDbContext --verbose & goto exit
IF "%1"=="update-context" dotnet ef database update -c CustomersDbContext --verbose --project .\ECommerce.Services.Customers\ECommerce.Services.Customers.csproj & goto exit 

:exit