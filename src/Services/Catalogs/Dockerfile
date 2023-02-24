# Using the base image of the Dockerfile for debugging can be more efficient because you don't need to build the entire application from scratch. Instead, you can reuse the already-built layers and add debugging tools and configurations as needed. This can save time and resources, especially if your application is large or complex.
# On the other hand, doing a full build for debugging can ensure that the debugging environment is identical to the production environment. This can help catch issues that may not surface in a modified version of the image, and provide a more accurate representation of the production environment. However, this approach can be slower and require more resources.

FROM mcr.microsoft.com/dotnet/aspnet:latest AS base
WORKDIR /app
#https://andrewlock.net/5-ways-to-set-the-urls-for-an-aspnetcore-app/
#https://swimburger.net/blog/dotnet/how-to-get-aspdotnet-core-server-urls
#https://tymisko.hashnode.dev/developing-aspnet-core-apps-in-docker-live-recompilat
#https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments
EXPOSE 80
EXPOSE 443
ENV ASPNETCORE_URLS http://*:80;https://*:443 
ENV ASPNETCORE_ENVIRONMENT docker

# # https://code.visualstudio.com/docs/containers/troubleshooting#_running-as-a-nonroot-user
# # https://baeldung.com/ops/root-user-password-docker-container
# # https://stackoverflow.com/questions/52070171/whats-the-default-user-for-docker-exec
# # https://medium.com/redbubble/running-a-docker-container-as-a-non-root-user-7d2e00f8ee15
# # Creates a non-root user with an explicit UID and adds permission to access the /app folder
# # if we don't define a user container will use root user
# RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
# USER appuser

FROM mcr.microsoft.com/dotnet/sdk:latest as build
WORKDIR /src
# path are related to build context, here for us build context is root folder
# https://docs.docker.com/build/building/context/
COPY ./.editorconfig ./

COPY ./src/Directory.Build.props ./
COPY ./src/Directory.Build.targets ./
COPY ./src/Directory.Packages.props ./
COPY ./src/Packages.props ./
COPY ./src/Services/Catalogs/Directory.Build.props ./Services/Catalogs/

# https://docs.docker.com/build/cache/#order-your-layers
# with any changes in csproj files all downstream layer will rebuil, so dotnet restore will execute again
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
COPY ./src/BuildingBlocks/BuildingBlocks.Messaging.Persistence.Postgres/BuildingBlocks.Messaging.Persistence.Postgres.csproj ./BuildingBlocks/BuildingBlocks.Messaging.Persistence.Postgres/
COPY ./src/BuildingBlocks/BuildingBlocks.OpenTelemetry/BuildingBlocks.OpenTelemetry.csproj ./BuildingBlocks/BuildingBlocks.OpenTelemetry/
COPY ./src/BuildingBlocks/BuildingBlocks.Resiliency/BuildingBlocks.Resiliency.csproj ./BuildingBlocks/BuildingBlocks.Resiliency/
COPY ./src/BuildingBlocks/BuildingBlocks.Security/BuildingBlocks.Security.csproj ./BuildingBlocks/BuildingBlocks.Security/
COPY ./src/BuildingBlocks/BuildingBlocks.Swagger/BuildingBlocks.Swagger.csproj ./BuildingBlocks/BuildingBlocks.Swagger/
COPY ./src/BuildingBlocks/BuildingBlocks.Validation/BuildingBlocks.Validation.csproj ./BuildingBlocks/BuildingBlocks.Validation/
COPY ./src/BuildingBlocks/BuildingBlocks.Web/BuildingBlocks.Web.csproj ./BuildingBlocks/BuildingBlocks.Web/

COPY ./src/Services/Catalogs/ECommerce.Services.Catalogs/ECommerce.Services.Catalogs.csproj ./Services/Catalogs/ECommerce.Services.Catalogs/
COPY ./src/Services/Catalogs/ECommerce.Services.Catalogs.Api/ECommerce.Services.Catalogs.Api.csproj ./Services/Catalogs/ECommerce.Services.Catalogs.Api/
COPY ./src/Services/Shared/ECommerce.Services.Shared/ECommerce.Services.Shared.csproj ./Services/Shared/ECommerce.Services.Shared/

# https://docs.docker.com/build/cache/
# https://docs.docker.com/build/cache/#order-your-layers
# https://github.com/moby/buildkit/blob/master/frontend/dockerfile/docs/reference.md#run---mounttypecache
# https://github.com/dotnet/dotnet-docker/issues/3353
# https://stackoverflow.com/questions/69464184/using-docker-buildkit-mount-type-cache-for-caching-nuget-packages-for-net-5-d
# https://pythonspeed.com/articles/docker-cache-pip-downloads/
# When we have a chnage in a layer that layer and all subsequent layer will rebuild again
# when installing packages, we don’t always need to fetch all of our packages from the internet each time. if we have any package update on `ECommerce.Services.Catalogs.Api.csproj` this layer will rebuild but it don't download all packages again, it just download new packages and for exisitng one uses mount cache 
RUN dotnet restore ./Services/Catalogs/ECommerce.Services.Catalogs.Api/ECommerce.Services.Catalogs.Api.csproj

# Copy project files
COPY ./src/BuildingBlocks/ ./BuildingBlocks/
COPY ./src/Services/Catalogs/ECommerce.Services.Catalogs.Api/  ./Services/Catalogs/ECommerce.Services.Catalogs.Api/
COPY ./src/Services/Catalogs/ECommerce.Services.Catalogs/  ./Services/Catalogs/ECommerce.Services.Catalogs/
COPY ./src/Services/Shared/  ./Services/Shared/

WORKDIR /src/Services/Catalogs/ECommerce.Services.Catalogs.Api/

RUN dotnet build -c Release --no-restore 

FROM build AS publish
# Publish project to output folder and no build and restore, as we did it already
# https://stackoverflow.com/questions/5457095/release-generating-pdb-files-why
# pdbs also generate for release mode (pdbonly) so vsdb can use it for debugging for debug mode its default is (full)
RUN dotnet publish -c Release --no-build --no-restore -o /app/publish

FROM base AS final
# Setup working directory for the project
WORKDIR /app
COPY --from=publish /app/publish  .

# for debug mode we change entrypoint with '--entrypoint' in 'docker run' for prevent runing application in this stage because we want to run container app with debugger launcher
#https://docs.docker.com/engine/reference/run/#entrypoint-default-command-to-execute-at-runtime
#https://oprea.rocks/blog/how-to-properly-override-the-entrypoint-using-docker-run

# https://andrewlock.net/5-ways-to-set-the-urls-for-an-aspnetcore-app/
# when we `run` app `dll`, inner `api project` working directory (will resolve to current working directory for app) that contains appsetings.json files or inner `bin directory` because when run app dll in this directory `app working directory` and `current working directory` will be set bin and because appsettings.json are there, so app can find this `appsettings.json` files in current working directory but if we run app dll outside this directories app current working directory will be changed, and it can't find `appsettings.json` files in current working directory, so we should explicitly specify working dir in to `bin` or `app project` folder, this problem doesn't exist for `.csproj files` and their working dir always resolve `correctly`
# in this layer we don't have nugets so we can use mounted volume in `docker run` or `docker-compose up` for this entrypoint when docker container will be run for the `host` with --mount type=bind,source=${env:USERPROFILE}\\.nuget\\packages,destination=/root/.nuget/packages,readonly, for example dotnet <application.dll> --additionalProbingPath /root/nuget/packages --additionalProbingPath ~/.nuget/packages 
ENTRYPOINT ["dotnet", "ECommerce.Services.Catalogs.Api.dll"]
