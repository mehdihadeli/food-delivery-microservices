FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

# Setup working directory for the project
WORKDIR /src

COPY ./src/Directory.Build.props ./
COPY ./src/Directory.Build.targets ./
COPY ./src/Packages.props ./
COPY ./src/ApiGateway/Directory.Build.props ./ApiGateway/
COPY ./src/ApiGateway/ECommerce.ApiGateway.csproj ./ApiGateway/ECommerce.ApiGateway/

# Restore nuget packages
RUN dotnet restore ./ApiGateway/ECommerce.ApiGateway/ECommerce.ApiGateway.csproj

# Copy project files
COPY ./src/ApiGateway/ECommerce.ApiGateway/  ./ApiGateway/ECommerce.ApiGateway/

# Build project with Release configuration
# and no restore, as we did it already
RUN dotnet build -c Release --no-restore  ./ApiGateway/ECommerce.ApiGateway/ECommerce.ApiGateway.csproj

WORKDIR /src/ApiGateway/ECommerce.ApiGateway/
# Publish project to output folder
# and no build, as we did it already
RUN ls
RUN dotnet publish -c Release --no-build -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
# Setup working directory for the project
WORKDIR /app
COPY --from=builder /src/ApiGateway/ECommerce.ApiGateway/out  .
EXPOSE 80
ENV ASPNETCORE_URLS http://*:80
ENV ASPNETCORE_ENVIRONMENT docker
ENTRYPOINT ["dotnet", "ECommerce.ApiGateway.dll"]
