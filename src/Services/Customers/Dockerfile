FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

# Setup working directory for the project
WORKDIR /src

COPY ./src/Directory.Build.props ./
COPY ./src/Directory.Build.targets ./
COPY ./src/Packages.props ./
COPY ./src/Services/Customers/Directory.Build.props ./Services/Customers/

COPY ./src/BuildingBlocks/*.csproj ./BuildingBlocks/
COPY ./src/Services/Customers/ECommerce.Services.Customers/ECommerce.Services.Customers.csproj ./Services/Customers/ECommerce.Services.Customers/
COPY ./src/Services/Customers/ECommerce.Services.Customers.Api/ECommerce.Services.Customers.Api.csproj ./Services/Customers/ECommerce.Services.Customers.Api/

# restore nuget packages
RUN dotnet restore ./Services/Customers/ECommerce.Services.Customers.Api/ECommerce.Services.Customers.Api.csproj

# Copy project files
COPY ./src/BuildingBlocks/ ./BuildingBlocks/
COPY ./src/Services/Customers/ECommerce.Services.Customers.Api/  ./Services/Customers/ECommerce.Services.Customers.Api/
COPY ./src/Services/Customers/ECommerce.Services.Customers/  ./Services/Customers/ECommerce.Services.Customers/

# Build project with Release configuration
# and no reecommerce, as we did it already
RUN dotnet build -c Release --no-reecommerce  ./Services/Customers/ECommerce.Services.Customers.Api/ECommerce.Services.Customers.Api.csproj

WORKDIR /src/Services/Customers/ECommerce.Services.Customers.Api/
# Publish project to output folder
# and no build, as we did it already
RUN ls
RUN dotnet publish -c Release --no-build -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
# Setup working directory for the project
WORKDIR /app
COPY --from=builder /src/Services/Customers/ECommerce.Services.Customers.Api/out  .
EXPOSE 80
ENV ASPNETCORE_URLS http://*:80
ENV ASPNETCORE_ENVIRONMENT docker
ENTRYPOINT ["dotnet", "ECommerce.Services.Customers.Api.dll"]
