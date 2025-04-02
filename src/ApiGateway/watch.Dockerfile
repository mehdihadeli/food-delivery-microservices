FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

# Setup working directory for the project
WORKDIR /src

COPY ./src/Directory.Build.props ./
COPY ./src/Directory.Build.targets ./
COPY ./src/Directory.Packages.props ./
COPY ./src/Packages.props ./

COPY ./src/ApiGateway/Directory.Build.props ./ApiGateway/
COPY ./src/ApiGateway/FoodDelivery.ApiGateway/FoodDelivery.ApiGateway.csproj ./ApiGateway/FoodDelivery.ApiGateway/

# Restore nuget packages
RUN dotnet restore ./ApiGateway/FoodDelivery.ApiGateway/FoodDelivery.ApiGateway.csproj

# Copy project files
COPY ./src/ApiGateway/FoodDelivery.ApiGateway/  ./ApiGateway/FoodDelivery.ApiGateway/

RUN ls

WORKDIR /src/ApiGateway/FoodDelivery.ApiGateway/

#https://andrewlock.net/5-ways-to-set-the-urls-for-an-aspnetcore-app/
#https://swimburger.net/blog/dotnet/how-to-get-aspdotnet-core-server-urls
#https://tymisko.hashnode.dev/developing-aspnet-core-apps-in-docker-live-recompilation

RUN dotnet watch run  FoodDelivery.ApiGateway.csproj --launch-profile ApiGateway.LiveRecompilation -p:SkipVersion=true
