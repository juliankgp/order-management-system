# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["services/LoggingService/src/Api/LoggingService.Api.csproj", "services/LoggingService/src/Api/"]
COPY ["services/LoggingService/src/Infrastructure/LoggingService.Infrastructure.csproj", "services/LoggingService/src/Infrastructure/"]
COPY ["services/LoggingService/src/Application/LoggingService.Application.csproj", "services/LoggingService/src/Application/"]
COPY ["services/LoggingService/src/Domain/LoggingService.Domain.csproj", "services/LoggingService/src/Domain/"]
COPY ["shared/OrderManagement.Shared.Common/OrderManagement.Shared.Common.csproj", "shared/OrderManagement.Shared.Common/"]
COPY ["shared/OrderManagement.Shared.Events/OrderManagement.Shared.Events.csproj", "shared/OrderManagement.Shared.Events/"]
COPY ["shared/OrderManagement.Shared.Security/OrderManagement.Shared.Security.csproj", "shared/OrderManagement.Shared.Security/"]
RUN dotnet restore "./services/LoggingService/src/Api/LoggingService.Api.csproj"
COPY . .
WORKDIR "/src/services/LoggingService/src/Api"
RUN dotnet build "./LoggingService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LoggingService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LoggingService.Api.dll"]