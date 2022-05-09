FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

# Setup working directory for the project
WORKDIR /src

COPY ./src/BuildingBlocks/micro-bootstrap/src/*.csproj ./BuildingBlocks/micro-bootstrap/src/
COPY ./src/Services/Store.Services.Customers/Store.Services.Customers/Store.Services.Customers.csproj ./Services/Store.Services.Customers/Store.Services.Customers/
COPY ./src/Services/Store.Services.Customers/Store.Services.Customers.Api/Store.Services.Customers.Api.csproj ./Services/Store.Services.Customers/Store.Services.Customers.Api/

# Restore nuget packages
RUN dotnet restore ./Services/Store.Services.Customers/src/Store.Services.Customers.Api/Store.Services.Customers.Api.csproj

# Copy project files
COPY ./src/BuildingBlocks/micro-bootstrap/src/ ./BuildingBlocks/micro-bootstrap/src/
COPY ./src/Services/Store.Services.Customers/Store.Services.Customers.Api/  ./Services/Store.Services.Customers/Store.Services.Customers.Api/
COPY ./src/Services/Store.Services.Customers/Store.Services.Customers/  ./Services/Store.Services.Customers/Store.Services.Customers/

# Build project with Release configuration
# and no restore, as we did it already
RUN dotnet build -c Release --no-restore  ./Services/Store.Services.Customers/Store.Services.Customers.Api/Store.Services.Customers.Api.csproj

WORKDIR /src/Services/Store.Services.Customers/Store.Services.Customers.Api/
# Publish project to output folder
# and no build, as we did it already
RUN ls
RUN dotnet publish -c Release --no-build -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
# Setup working directory for the project
WORKDIR /app
COPY --from=builder /src/Services/Store.Services.Customers/Store.Services.Customers.Api/out  .
EXPOSE 80

ENV ASPNETCORE_URLS http://*:80
ENV ASPNETCORE_ENVIRONMENT docker
ENTRYPOINT ["dotnet", "Store.Services.Customers.Api.dll"]
