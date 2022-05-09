FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

# Setup working directory for the project
WORKDIR /src

COPY ./src/ApiGateway/Store.ApiGateway/src/Store.ApiGateway/Store.ApiGateway.csproj ./ApiGateway/Store.ApiGateway/src/Store.ApiGateway/

# Restore nuget packages
RUN dotnet restore ./ApiGateway/Store.ApiGateway/src/Store.ApiGateway/Store.ApiGateway.csproj

# Copy project files
COPY ./src/ApiGateway/Store.ApiGateway/src/Store.ApiGateway/  ./ApiGateway/Store.ApiGateway/src/Store.ApiGateway/

# Build project with Release configuration
# and no restore, as we did it already
RUN dotnet build -c Release --no-restore  ./ApiGateway/Store.ApiGateway/src/Store.ApiGateway/Store.ApiGateway.csproj

WORKDIR /src/ApiGateway/Store.ApiGateway/src/Store.ApiGateway/
# Publish project to output folder
# and no build, as we did it already
RUN ls
RUN dotnet publish -c Release --no-build -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
# Setup working directory for the project
WORKDIR /app
COPY --from=builder /src/ApiGateway/Store.ApiGateway/src/Store.ApiGateway/out  .
EXPOSE 80
ENV ASPNETCORE_URLS http://*:80
ENV ASPNETCORE_ENVIRONMENT docker
ENTRYPOINT ["dotnet", "Store.ApiGateway.dll"]
