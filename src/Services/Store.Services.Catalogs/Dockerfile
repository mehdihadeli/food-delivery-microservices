FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

# Setup working directory for the project
WORKDIR /src

COPY ./src/BuildingBlocks/micro-bootstrap/src/*.csproj ./BuildingBlocks/micro-bootstrap/src/
COPY ./src/Services/Store.Services.Catalogs/Store.Services.Catalogs/Store.Services.Catalogs.csproj ./Services/Store.Services.Catalogs/Store.Services.Catalogs/
COPY ./src/Services/Store.Services.Catalogs/Store.Services.Catalogs.Api/Store.Services.Catalogs.Api.csproj ./Services/Store.Services.Catalogs/Store.Services.Catalogs.Api/

# Restore nuget packages
RUN dotnet restore ./Services/Store.Services.Catalogs/Store.Services.Catalogs.Api/Store.Services.Catalogs.Api.csproj

# Copy project files
COPY ./src/BuildingBlocks/micro-bootstrap/src/ ./BuildingBlocks/micro-bootstrap/src/
COPY ./src/Services/Store.Services.Catalogs/Store.Services.Catalogs.Api/  ./Services/Store.Services.Catalogs/Store.Services.Catalogs.Api/
COPY ./src/Services/Store.Services.Catalogs/Store.Services.Catalogs/  ./Services/Store.Services.Catalogs/Store.Services.Catalogs/

# Build project with Release configuration
# and no restore, as we did it already
RUN dotnet build -c Release --no-restore  ./Services/Store.Services.Catalogs/Store.Services.Catalogs.Api/Store.Services.Catalogs.Api.csproj

WORKDIR /src/Services/Store.Services.Catalogs/Store.Services.Catalogs.Api/
# Publish project to output folder
# and no build, as we did it already
RUN ls
RUN dotnet publish -c Release --no-build -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
# Setup working directory for the project
WORKDIR /app
COPY --from=builder /src/Services/Store.Services.Catalogs/Store.Services.Catalogs.Api/out  .
EXPOSE 80
ENV ASPNETCORE_URLS http://*:80
ENV ASPNETCORE_ENVIRONMENT docker
ENTRYPOINT ["dotnet", "Store.Services.Catalogs.Api.dll"]
