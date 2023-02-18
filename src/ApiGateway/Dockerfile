# Using the base image of the Dockerfile for debugging can be more efficient because you don't need to build the entire application from scratch. Instead, you can reuse the already-built layers and add debugging tools and configurations as needed. This can save time and resources, especially if your application is large or complex.
# On the other hand, doing a full build for debugging can ensure that the debugging environment is identical to the production environment. This can help catch issues that may not surface in a modified version of the image, and provide a more accurate representation of the production environment. However, this approach can be slower and require more resources.

FROM mcr.microsoft.com/dotnet/aspnet:latest AS base
WORKDIR /app
#https://andrewlock.net/5-ways-to-set-the-urls-for-an-aspnetcore-app/
#https://swimburger.net/blog/dotnet/how-to-get-aspdotnet-core-server-urls
#https://tymisko.hashnode.dev/developing-aspnet-core-apps-in-docker-live-recompilat
#https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments
EXPOSE 80
ENV ASPNETCORE_URLS http://*:80
ENV ASPNETCORE_ENVIRONMENT docker

FROM mcr.microsoft.com/dotnet/sdk:latest AS builder
# Setup working directory for the project
WORKDIR /src

COPY ./src/Directory.Build.props ./
COPY ./src/Directory.Build.targets ./
COPY ./src/Directory.Packages.props ./
COPY ./src/Packages.props ./

COPY ./src/ApiGateway/Directory.Build.props ./ApiGateway/
COPY ./src/ApiGateway/ECommerce.ApiGateway/ECommerce.ApiGateway.csproj ./ApiGateway/ECommerce.ApiGateway/

# Restore nuget packages
RUN dotnet restore ./ApiGateway/ECommerce.ApiGateway/ECommerce.ApiGateway.csproj

# Copy project files
COPY ./src/ApiGateway/ECommerce.ApiGateway/  ./ApiGateway/ECommerce.ApiGateway/

# Build project with Release configuration
# and no restore, as we did it already
RUN dotnet build -c Release --no-restore  ./ApiGateway/ECommerce.ApiGateway/ECommerce.ApiGateway.csproj

WORKDIR /src/ApiGateway/ECommerce.ApiGateway/
# Publish project to output folder
# and no build and restore, as we did it already

# Publish project to output folder and no build and restore, as we did it already
# https://stackoverflow.com/questions/5457095/release-generating-pdb-files-why
# pdbs also generate for release mode (pdb-only) so vsdb can use it for debugging for debug mode its default is (full)
RUN dotnet publish -c Release --no-build --no-restore -o out

FROM base AS final
# Setup working directory for the project
WORKDIR /app
COPY --from=builder /src/ApiGateway/ECommerce.ApiGateway/out  .

# for debug mode we change entrypoint with '--entrypoint' in 'docker run' for prevent runing application in this stage because we want to run container app with debugger launcher
#https://docs.docker.com/engine/reference/run/#entrypoint-default-command-to-execute-at-runtime
#https://oprea.rocks/blog/how-to-properly-override-the-entrypoint-using-docker-run

#https://andrewlock.net/5-ways-to-set-the-urls-for-an-aspnetcore-app/
ENTRYPOINT ["dotnet", "ECommerce.ApiGateway.dll"]
