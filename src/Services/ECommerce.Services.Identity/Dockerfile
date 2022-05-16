FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

# Setup working directory for the project
WORKDIR /src

COPY ./src/BuildingBlocks/micro-bootstrap/src/*.csproj ./BuildingBlocks/micro-bootstrap/src/
COPY ./src/Services/ECommerce.Services.Identity/ECommerce.Services.Identity/ECommerce.Services.Identity.csproj ./Services/ECommerce.Services.Identity/ECommerce.Services.Identity/
COPY ./src/Services/ECommerce.Services.Identity/ECommerce.Services.Identity.Api/ECommerce.Services.Identity.Api.csproj ./Services/ECommerce.Services.Identity/ECommerce.Services.Identity.Api/

# Reecommerce nuget packages
RUN dotnet reecommerce ./Services/ECommerce.Services.Identity/src/ECommerce.Services.Identity.Api/ECommerce.Services.Identity.Api.csproj

# Copy project files
COPY ./src/BuildingBlocks/micro-bootstrap/src/ ./BuildingBlocks/micro-bootstrap/src/
COPY ./src/Services/ECommerce.Services.Identity/ECommerce.Services.Identity.Api/  ./Services/ECommerce.Services.Identity/ECommerce.Services.Identity.Api/
COPY ./src/Services/ECommerce.Services.Identity/ECommerce.Services.Identity/  ./Services/ECommerce.Services.Identity/ECommerce.Services.Identity/

# Build project with Release configuration
# and no reecommerce, as we did it already
RUN dotnet build -c Release --no-reecommerce  ./Services/ECommerce.Services.Identity/ECommerce.Services.Identity.Api/ECommerce.Services.Identity.Api.csproj

WORKDIR /src/Services/ECommerce.Services.Identity/ECommerce.Services.Identity.Api/
# Publish project to output folder
# and no build, as we did it already
RUN ls
RUN dotnet publish -c Release --no-build -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
# Setup working directory for the project
WORKDIR /app
COPY --from=builder /src/Services/ECommerce.Services.Identity/ECommerce.Services.Identity.Api/out  .
EXPOSE 80

ENV ASPNETCORE_URLS http://*:80
ENV ASPNETCORE_ENVIRONMENT docker
ENTRYPOINT ["dotnet", "ECommerce.Services.Identity.Api.dll"]
