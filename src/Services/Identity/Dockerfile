# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=docker


FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG TARGETARCH
WORKDIR /src

COPY ./.editorconfig ./
COPY ./nuget.config ./
COPY ./global.json ./

COPY ./Directory.Build.props ../
COPY ./Directory.Packages.props ../
COPY ./src/Directory.Build.props ./
COPY ./src/Directory.Build.targets ./
COPY ./src/Services/Identity/Directory.Build.props ./Services/Identity/

COPY ./src/BuildingBlocks/Directory.Build.props ./BuildingBlocks/
COPY ./src/BuildingBlocks/Directory.Build.targets ./BuildingBlocks/
COPY ./src/BuildingBlocks/Directory.Packages.props ./BuildingBlocks/
COPY ./src/BuildingBlocks/src/Directory.Build.props ./BuildingBlocks/src/

COPY ./src/BuildingBlocks/src/Squirrel.Abstractions/Squirrel.Abstractions.csproj ./BuildingBlocks/src/Squirrel.Abstractions/
COPY ./src/BuildingBlocks/src/Squirrel.Core/Squirrel.Core.csproj ./BuildingBlocks/src/Squirrel.Core/
COPY ./src/BuildingBlocks/src/Squirrel.HealthCheck/Squirrel.HealthCheck.csproj ./BuildingBlocks/src/Squirrel.HealthCheck/
COPY ./src/BuildingBlocks/src/Squirrel.OpenTelemetry/Squirrel.OpenTelemetry.csproj ./BuildingBlocks/src/Squirrel.OpenTelemetry/
COPY ./src/BuildingBlocks/src/Squirrel.Resiliency/Squirrel.Resiliency.csproj ./BuildingBlocks/src/Squirrel.Resiliency/
COPY ./src/BuildingBlocks/src/Squirrel.Web/Squirrel.Web.csproj ./BuildingBlocks/src/Squirrel.Web/
COPY ./src/BuildingBlocks/src/Squirrel.Caching/Squirrel.Caching.csproj ./BuildingBlocks/src/Squirrel.Caching/
COPY ./src/BuildingBlocks/src/Squirrel.Email/Squirrel.Email.csproj ./BuildingBlocks/src/Squirrel.Email/
COPY ./src/BuildingBlocks/src/Squirrel.Integration.Wolverine/Squirrel.Integration.Wolverine.csproj ./BuildingBlocks/src/Squirrel.Integration.Wolverine/
COPY ./src/BuildingBlocks/src/Squirrel.OpenApi/Squirrel.OpenApi.csproj ./BuildingBlocks/src/Squirrel.OpenApi/
COPY ./src/BuildingBlocks/src/Squirrel.Persistence.EfCore.Postgres/Squirrel.Persistence.EfCore.Postgres.csproj ./BuildingBlocks/src/Squirrel.Persistence.EfCore.Postgres/
COPY ./src/BuildingBlocks/src/Squirrel.Persistence.Mongo/Squirrel.Persistence.Mongo.csproj ./BuildingBlocks/src/Squirrel.Persistence.Mongo/
COPY ./src/BuildingBlocks/src/Squirrel.SerilogLogging/Squirrel.SerilogLogging.csproj ./BuildingBlocks/src/Squirrel.SerilogLogging/
COPY ./src/BuildingBlocks/src/Squirrel.Validation/Squirrel.Validation.csproj ./BuildingBlocks/src/Squirrel.Validation/
COPY ./src/Aspire/FoodDelivery.ServiceDefaults/FoodDelivery.ServiceDefaults.csproj ./Aspire/FoodDelivery.ServiceDefaults/

COPY ./src/Services/Identity/FoodDelivery.Services.Identity/FoodDelivery.Services.Identity.csproj ./Services/Identity/FoodDelivery.Services.Identity/
COPY ./src/Services/Identity/FoodDelivery.Services.Identity.Api/FoodDelivery.Services.Identity.Api.csproj ./Services/Identity/FoodDelivery.Services.Identity.Api/
COPY ./src/Services/Shared/FoodDelivery.Services.Shared/FoodDelivery.Services.Shared.csproj ./Services/Shared/FoodDelivery.Services.Shared/


RUN dotnet restore ./Services/Identity/FoodDelivery.Services.Identity.Api/FoodDelivery.Services.Identity.Api.csproj -p:SkipVersion=true -a $TARGETARCH

COPY ./src/BuildingBlocks/ ./BuildingBlocks/
COPY ./src/Aspire/FoodDelivery.ServiceDefaults/ ./Aspire/FoodDelivery.ServiceDefaults/
COPY ./src/Services/Identity/FoodDelivery.Services.Identity.Api/  ./Services/Identity/FoodDelivery.Services.Identity.Api/
COPY ./src/Services/Identity/FoodDelivery.Services.Identity/  ./Services/Identity/FoodDelivery.Services.Identity/
COPY ./src/Services/Shared/  ./Services/Shared/

WORKDIR /src/Services/Identity/FoodDelivery.Services.Identity.Api/

RUN dotnet build -c Release --no-restore -p:SkipVersion=true -a $TARGETARCH

FROM build AS publish
RUN dotnet publish -c Release --no-build --no-restore -p:SkipVersion=true -a $TARGETARCH -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish  .
USER $APP_UID


ENTRYPOINT ["dotnet", "FoodDelivery.Services.Identity.Api.dll"]
