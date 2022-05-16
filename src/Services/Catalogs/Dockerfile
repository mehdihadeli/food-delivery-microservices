FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

# Setup working directory for the project
WORKDIR /src

COPY ./src/Directory.Build.props ./
COPY ./src/Directory.Build.targets ./
COPY ./src/Packages.props ./
COPY ./src/Services/Catalogs/Directory.Build.props ./Services/Catalogs/

COPY ./src/BuildingBlocks/*.csproj ./BuildingBlocks/
COPY ./src/Services/Catalogs/ECommerce.Services.Catalogs/ECommerce.Services.Catalogs.csproj ./Services/Catalogs/ECommerce.Services.Catalogs/
COPY ./src/Services/Catalogs/ECommerce.Services.Catalogs.Api/ECommerce.Services.Catalogs.Api.csproj ./Services/Catalogs/ECommerce.Services.Catalogs.Api/

# restore nuget packages
RUN dotnet restore ./Services/Catalogs/ECommerce.Services.Catalogs.Api/ECommerce.Services.Catalogs.Api.csproj

# Copy project files
COPY ./src/BuildingBlocks/ ./BuildingBlocks/
COPY ./src/Services/Catalogs/ECommerce.Services.Catalogs.Api/  ./Services/Catalogs/ECommerce.Services.Catalogs.Api/
COPY ./src/Services/Catalogs/ECommerce.Services.Catalogs/  ./Services/Catalogs/ECommerce.Services.Catalogs/

# Build project with Release configuration
# and no reecommerce, as we did it already
RUN dotnet build -c Release --no-reecommerce  ./Services/Catalogs/ECommerce.Services.Catalogs.Api/ECommerce.Services.Catalogs.Api.csproj

WORKDIR /src/Services/Catalogs/ECommerce.Services.Catalogs.Api/
# Publish project to output folder
# and no build, as we did it already
RUN ls
RUN dotnet publish -c Release --no-build -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
# Setup working directory for the project
WORKDIR /app
COPY --from=builder /src/Services/Catalogs/ECommerce.Services.Catalogs.Api/out  .
EXPOSE 80
ENV ASPNETCORE_URLS http://*:80
ENV ASPNETCORE_ENVIRONMENT docker
ENTRYPOINT ["dotnet", "ECommerce.Services.Catalogs.Api.dll"]
