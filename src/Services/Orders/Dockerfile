FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

# Setup working directory for the project
WORKDIR /src

COPY ./src/Directory.Build.props ./
COPY ./src/Directory.Build.targets ./
COPY ./src/Packages.props ./
COPY ./src/Services/Orders/Directory.Build.props ./Services/Orders/

COPY ./src/BuildingBlocks/*.csproj ./BuildingBlocks/
COPY ./src/Services/Orders/ECommerce.Services.Orders/ECommerce.Services.Orders.csproj ./Services/Orders/ECommerce.Services.Orders/
COPY ./src/Services/Orders/ECommerce.Services.Orders.Api/ECommerce.Services.Orders.Api.csproj ./Services/Orders/ECommerce.Services.Orders.Api/

# restore nuget packages
RUN dotnet restore ./Services/Orders/ECommerce.Services.Orders.Api/ECommerce.Services.Orders.Api.csproj

# Copy project files
COPY ./src/BuildingBlocks/ ./BuildingBlocks/
COPY ./src/Services/Orders/ECommerce.Services.Orders.Api/  ./Services/Orders/ECommerce.Services.Orders.Api/
COPY ./src/Services/Orders/ECommerce.Services.Orders/  ./Services/Orders/ECommerce.Services.Orders/

# Build project with Release configuration
# and no reecommerce, as we did it already
RUN dotnet build -c Release --no-reecommerce  ./Services/Orders/ECommerce.Services.Orders.Api/ECommerce.Services.Orders.Api.csproj

WORKDIR /src/Services/Orders/ECommerce.Services.Orders.Api/
# Publish project to output folder
# and no build, as we did it already
RUN ls
RUN dotnet publish -c Release --no-build -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
# Setup working directory for the project
WORKDIR /app
COPY --from=builder /src/Services/Orders/ECommerce.Services.Orders.Api/out  .
EXPOSE 80
ENV ASPNETCORE_URLS http://*:80
ENV ASPNETCORE_ENVIRONMENT docker
ENTRYPOINT ["dotnet", "ECommerce.Services.Orders.Api.dll"]
