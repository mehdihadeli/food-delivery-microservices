#https://tymisko.hashnode.dev/developing-aspnet-core-apps-in-docker-live-recompilation
FROM mcr.microsoft.com/dotnet/sdk:latest as builder

WORKDIR /src

COPY ./.editorconfig ./

COPY ./src/Directory.Build.props ./
COPY ./src/Directory.Build.targets ./
COPY ./src/Directory.Packages.props ./
COPY ./src/Packages.props ./
COPY ./src/Services/Orders/Directory.Build.props ./Services/Orders/

# TODO: Using wildcard to copy all files in the directory.
COPY ./src/BuildingBlocks/BuildingBlocks.Abstractions/BuildingBlocks.Abstractions.csproj ./BuildingBlocks/BuildingBlocks.Abstractions/
COPY ./src/BuildingBlocks/BuildingBlocks.Core/BuildingBlocks.Core.csproj ./BuildingBlocks/BuildingBlocks.Core/
COPY ./src/BuildingBlocks/BuildingBlocks.Caching/BuildingBlocks.Caching.csproj ./BuildingBlocks/BuildingBlocks.Caching/
COPY ./src/BuildingBlocks/BuildingBlocks.Email/BuildingBlocks.Email.csproj ./BuildingBlocks/BuildingBlocks.Email/
COPY ./src/BuildingBlocks/BuildingBlocks.Integration.MassTransit/BuildingBlocks.Integration.MassTransit.csproj ./BuildingBlocks/BuildingBlocks.Integration.MassTransit/
COPY ./src/BuildingBlocks/BuildingBlocks.Logging/BuildingBlocks.Logging.csproj ./BuildingBlocks/BuildingBlocks.Logging/
COPY ./src/BuildingBlocks/BuildingBlocks.HealthCheck/BuildingBlocks.HealthCheck.csproj ./BuildingBlocks/BuildingBlocks.HealthCheck/
COPY ./src/BuildingBlocks/BuildingBlocks.Persistence.EfCore.Postgres/BuildingBlocks.Persistence.EfCore.Postgres.csproj ./BuildingBlocks/BuildingBlocks.Persistence.EfCore.Postgres/
COPY ./src/BuildingBlocks/BuildingBlocks.Persistence.Mongo/BuildingBlocks.Persistence.Mongo.csproj ./BuildingBlocks/BuildingBlocks.Persistence.Mongo/
COPY ./src/BuildingBlocks/BuildingBlocks.Resiliency/BuildingBlocks.Resiliency.csproj ./BuildingBlocks/BuildingBlocks.Resiliency/
COPY ./src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.csproj ./BuildingBlocks/BuildingBlocks.Security/
COPY ./src/BuildingBlocks/BuildingBlocks.Swagger/BuildingBlocks.Swagger.csproj ./BuildingBlocks/BuildingBlocks.Swagger/
COPY ./src/BuildingBlocks/BuildingBlocks.Validation/BuildingBlocks.Validation.csproj ./BuildingBlocks/BuildingBlocks.Validation/
COPY ./src/BuildingBlocks/BuildingBlocks.Web/BuildingBlocks.Web.csproj ./BuildingBlocks/BuildingBlocks.Web/
COPY ./src/BuildingBlocks/BuildingBlocks.Messaging.Persistence.Postgres/BuildingBlocks.Messaging.Persistence.Postgres.csproj ./BuildingBlocks/BuildingBlocks.Messaging.Persistence.Postgres/
COPY ./src/BuildingBlocks/BuildingBlocks.OpenTelemetry/BuildingBlocks.OpenTelemetry.csproj ./BuildingBlocks/BuildingBlocks.OpenTelemetry/

# Copy project files
COPY ./src/BuildingBlocks/ ./BuildingBlocks/
COPY ./src/Services/Orders/ECommerce.Services.Orders.Api/  ./Services/Orders/ECommerce.Services.Orders.Api/
COPY ./src/Services/Orders/ECommerce.Services.Orders/  ./Services/Orders/ECommerce.Services.Orders/
COPY ./src/Services/Shared/  ./Services/Shared/

WORKDIR /src/Services/Orders/ECommerce.Services.Orders.Api/

# https://learn.microsoft.com/en-us/aspnet/core/tutorials/dotnet-watch?view=aspnetcore-7.0#dotnet-watch-configuration
# https://learn.microsoft.com/en-us/aspnet/core/fundamentals/file-providers?view=aspnetcore-3.1#watch-for-changes
# https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-watch?WT.mc_id=DOP-MVP-5001942#environment-variables
# Some file systems, such as Docker containers and network shares, may not reliably send change notifications. Set the DOTNET_USE_POLLING_FILE_WATCHER environment variable to 1 or true to poll the file system for changes every four seconds
# If set to "1" or "true", dotnet watch uses a polling file watcher instead of CoreFx's FileSystemWatcher. Used when watching files on network shares or Docker mounted volumes.
ENV DOTNET_USE_POLLING_FILE_WATCHER 1

RUN dotnet watch run  ECommerce.Services.Orders.Api.csproj --launch-profile Orders.Api.LiveRecompilation

