
IF "%1"=="init-context" dotnet ef migrations add InitialCustomersMigration -o \FoodDelivery.Services.Customer\Shared\Data\Migrations\Customer --project .\FoodDelivery.Services.Customers\FoodDelivery.Services.Customers.csproj -c CustomersDbContext --verbose & goto exit
IF "%1"=="update-context" dotnet ef database update -c CustomersDbContext --verbose --project .\FoodDelivery.Services.Customers\FoodDelivery.Services.Customers.csproj & goto exit 

:exit