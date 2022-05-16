FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

# Setup working directory for the project
WORKDIR /src

COPY ./src/Directory.Build.props ./
COPY ./src/Directory.Build.targets ./
COPY ./src/Packages.props ./
COPY ./src/Services/Identity/Directory.Build.props ./Services/Identity/

COPY ./src/BuildingBlocks/*.csproj ./BuildingBlocks/
COPY ./src/Services/Identity/ECommerce.Services.Identity/ECommerce.Services.Identity.csproj ./Services/Identity/ECommerce.Services.Identity/
COPY ./src/Services/Identity/ECommerce.Services.Identity.Api/ECommerce.Services.Identity.Api.csproj ./Services/Identity/ECommerce.Services.Identity.Api/

# restore nuget packages
RUN dotnet restore ./Services/Identity/ECommerce.Services.Identity.Api/ECommerce.Services.Identity.Api.csproj

# Copy project files
COPY ./src/BuildingBlocks/ ./BuildingBlocks/
COPY ./src/Services/Identity/ECommerce.Services.Identity.Api/  ./Services/Identity/ECommerce.Services.Identity.Api/
COPY ./src/Services/Identity/ECommerce.Services.Identity/  ./Services/Identity/ECommerce.Services.Identity/

# Build project with Release configuration
# and no reecommerce, as we did it already
RUN dotnet build -c Release --no-reecommerce  ./Services/Identity/ECommerce.Services.Identity.Api/ECommerce.Services.Identity.Api.csproj

WORKDIR /src/Services/Identity/ECommerce.Services.Identity.Api/
# Publish project to output folder
# and no build, as we did it already
RUN ls
RUN dotnet publish -c Release --no-build -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
# Setup working directory for the project
WORKDIR /app
COPY --from=builder /src/Services/Identity/ECommerce.Services.Identity.Api/out  .
EXPOSE 80
ENV ASPNETCORE_URLS http://*:80
ENV ASPNETCORE_ENVIRONMENT docker
ENTRYPOINT ["dotnet", "ECommerce.Services.Identity.Api.dll"]
