FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

# Setup working directory for the project
WORKDIR /src

COPY ./src/BuildingBlocks/micro-bootstrap/src/*.csproj ./BuildingBlocks/micro-bootstrap/src/
COPY ./src/Services/Store.Services.Identity/Store.Services.Identity/Store.Services.Identity.csproj ./Services/Store.Services.Identity/Store.Services.Identity/
COPY ./src/Services/Store.Services.Identity/Store.Services.Identity.Api/Store.Services.Identity.Api.csproj ./Services/Store.Services.Identity/Store.Services.Identity.Api/

# Restore nuget packages
RUN dotnet restore ./Services/Store.Services.Identity/src/Store.Services.Identity.Api/Store.Services.Identity.Api.csproj

# Copy project files
COPY ./src/BuildingBlocks/micro-bootstrap/src/ ./BuildingBlocks/micro-bootstrap/src/
COPY ./src/Services/Store.Services.Identity/Store.Services.Identity.Api/  ./Services/Store.Services.Identity/Store.Services.Identity.Api/
COPY ./src/Services/Store.Services.Identity/Store.Services.Identity/  ./Services/Store.Services.Identity/Store.Services.Identity/

# Build project with Release configuration
# and no restore, as we did it already
RUN dotnet build -c Release --no-restore  ./Services/Store.Services.Identity/Store.Services.Identity.Api/Store.Services.Identity.Api.csproj

WORKDIR /src/Services/Store.Services.Identity/Store.Services.Identity.Api/
# Publish project to output folder
# and no build, as we did it already
RUN ls
RUN dotnet publish -c Release --no-build -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
# Setup working directory for the project
WORKDIR /app
COPY --from=builder /src/Services/Store.Services.Identity/Store.Services.Identity.Api/out  .
EXPOSE 80

ENV ASPNETCORE_URLS http://*:80
ENV ASPNETCORE_ENVIRONMENT docker
ENTRYPOINT ["dotnet", "Store.Services.Identity.Api.dll"]
