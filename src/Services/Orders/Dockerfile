FROM mcr.microsoft.com/dotnet/sdk:latest as builder

WORKDIR /src

COPY ./.editorconfig ./

COPY ./src/Directory.Build.props ./
COPY ./src/Directory.Build.targets ./
COPY ./src/Packages.props ./
COPY ./src/Services/Orders/Directory.Build.props ./Services/Orders/

# TODO: Using wildcard to copy all files in the directory.
COPY ./src/BuildingBlocks/BuildingBlocks.Abstractions/BuildingBlocks.Abstractions.csproj ./BuildingBlocks/BuildingBlocks.Abstractions/
COPY ./src/BuildingBlocks/BuildingBlocks.Core/BuildingBlocks.Core.csproj ./BuildingBlocks/BuildingBlocks.Core/
COPY ./src/BuildingBlocks/BuildingBlocks.Caching.InMemory/BuildingBlocks.Caching.InMemory.csproj ./BuildingBlocks/BuildingBlocks.Caching.InMemory/
COPY ./src/BuildingBlocks/BuildingBlocks.Email/BuildingBlocks.Email.csproj ./BuildingBlocks/BuildingBlocks.Email/
COPY ./src/BuildingBlocks/BuildingBlocks.Integration.MassTransit/BuildingBlocks.Integration.MassTransit.csproj ./BuildingBlocks/BuildingBlocks.Integration.MassTransit/
COPY ./src/BuildingBlocks/BuildingBlocks.Logging/BuildingBlocks.Logging.csproj ./BuildingBlocks/BuildingBlocks.Logging/
COPY ./src/BuildingBlocks/BuildingBlocks.Monitoring/BuildingBlocks.Monitoring.csproj ./BuildingBlocks/BuildingBlocks.Monitoring/
COPY ./src/BuildingBlocks/BuildingBlocks.Persistence.EfCore.Postgres/BuildingBlocks.Persistence.EfCore.Postgres.csproj ./BuildingBlocks/BuildingBlocks.Persistence.EfCore.Postgres/
COPY ./src/BuildingBlocks/BuildingBlocks.Persistence.Mongo/BuildingBlocks.Persistence.Mongo.csproj ./BuildingBlocks/BuildingBlocks.Persistence.Mongo/
COPY ./src/BuildingBlocks/BuildingBlocks.Resiliency/BuildingBlocks.Resiliency.csproj ./BuildingBlocks/BuildingBlocks.Resiliency/
COPY ./src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.csproj ./BuildingBlocks/BuildingBlocks.Security/
COPY ./src/BuildingBlocks/BuildingBlocks.Swagger/BuildingBlocks.Swagger.csproj ./BuildingBlocks/BuildingBlocks.Swagger/
COPY ./src/BuildingBlocks/BuildingBlocks.Validation/BuildingBlocks.Validation.csproj ./BuildingBlocks/BuildingBlocks.Validation/
COPY ./src/BuildingBlocks/BuildingBlocks.Web/BuildingBlocks.Web.csproj ./BuildingBlocks/BuildingBlocks.Web/

COPY ./src/Services/Orders/ECommerce.Services.Orders/ECommerce.Services.Orders.csproj ./Services/Orders/ECommerce.Services.Orders/
COPY ./src/Services/Orders/ECommerce.Services.Orders.Api/ECommerce.Services.Orders.Api.csproj ./Services/Orders/ECommerce.Services.Orders.Api/
COPY ./src/Services/Shared/ECommerce.Services.Shared/ECommerce.Services.Shared.csproj ./Services/Shared/ECommerce.Services.Shared/

RUN ls

# restore nuget packages
RUN dotnet restore ./Services/Orders/ECommerce.Services.Orders.Api/ECommerce.Services.Orders.Api.csproj

# Copy project files
COPY ./src/BuildingBlocks/ ./BuildingBlocks/
COPY ./src/Services/Orders/ECommerce.Services.Orders.Api/  ./Services/Orders/ECommerce.Services.Orders.Api/
COPY ./src/Services/Orders/ECommerce.Services.Orders/  ./Services/Orders/ECommerce.Services.Orders/
COPY ./src/Services/Shared/  ./Services/Shared/

# Build project with Release configuration
# and no restore, as we did it already
RUN dotnet build -c Release --no-restore  ./Services/Orders/ECommerce.Services.Orders.Api/ECommerce.Services.Orders.Api.csproj

WORKDIR /src/Services/Orders/ECommerce.Services.Orders.Api/
# Publish project to output folder
# and no build, as we did it already
RUN ls
RUN dotnet publish -c Release --no-build -o out

FROM mcr.microsoft.com/dotnet/aspnet:latest
# Setup working directory for the project
WORKDIR /app
COPY --from=builder /src/Services/Orders/ECommerce.Services.Orders.Api/out  .

#https://andrewlock.net/5-ways-to-set-the-urls-for-an-aspnetcore-app/
#https://swimburger.net/blog/dotnet/how-to-get-aspdotnet-core-server-urls
#https://tymisko.hashnode.dev/developing-aspnet-core-apps-in-docker-live-recompilat

EXPOSE 80
ENV ASPNETCORE_URLS http://*:80
ENV ASPNETCORE_ENVIRONMENT docker
ENTRYPOINT ["dotnet", "ECommerce.Services.Orders.Api.dll"]
