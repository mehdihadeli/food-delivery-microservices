FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

# Setup working directory for the project
WORKDIR /src

COPY ./src/BuildingBlocks/micro-bootstrap/src/*.csproj ./BuildingBlocks/micro-bootstrap/src/
COPY ./src/Services/ECommerce.Services.Catalogs/ECommerce.Services.Catalogs/ECommerce.Services.Catalogs.csproj ./Services/ECommerce.Services.Catalogs/ECommerce.Services.Catalogs/
COPY ./src/Services/ECommerce.Services.Catalogs/ECommerce.Services.Catalogs.Api/ECommerce.Services.Catalogs.Api.csproj ./Services/ECommerce.Services.Catalogs/ECommerce.Services.Catalogs.Api/

# Reecommerce nuget packages
RUN dotnet reecommerce ./Services/ECommerce.Services.Catalogs/ECommerce.Services.Catalogs.Api/ECommerce.Services.Catalogs.Api.csproj

# Copy project files
COPY ./src/BuildingBlocks/micro-bootstrap/src/ ./BuildingBlocks/micro-bootstrap/src/
COPY ./src/Services/ECommerce.Services.Catalogs/ECommerce.Services.Catalogs.Api/  ./Services/ECommerce.Services.Catalogs/ECommerce.Services.Catalogs.Api/
COPY ./src/Services/ECommerce.Services.Catalogs/ECommerce.Services.Catalogs/  ./Services/ECommerce.Services.Catalogs/ECommerce.Services.Catalogs/

# Build project with Release configuration
# and no reecommerce, as we did it already
RUN dotnet build -c Release --no-reecommerce  ./Services/ECommerce.Services.Catalogs/ECommerce.Services.Catalogs.Api/ECommerce.Services.Catalogs.Api.csproj

WORKDIR /src/Services/ECommerce.Services.Catalogs/ECommerce.Services.Catalogs.Api/
# Publish project to output folder
# and no build, as we did it already
RUN ls
RUN dotnet publish -c Release --no-build -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
# Setup working directory for the project
WORKDIR /app
COPY --from=builder /src/Services/ECommerce.Services.Catalogs/ECommerce.Services.Catalogs.Api/out  .
EXPOSE 80
ENV ASPNETCORE_URLS http://*:80
ENV ASPNETCORE_ENVIRONMENT docker
ENTRYPOINT ["dotnet", "ECommerce.Services.Catalogs.Api.dll"]
