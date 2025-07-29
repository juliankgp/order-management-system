# Dockerfile para OrderService

# Usar la imagen base de .NET Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001
EXPOSE 5081

# Usar la imagen del SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["services/OrderService/src/Api/OrderService.Api.csproj", "services/OrderService/src/Api/"]
COPY ["services/OrderService/src/Application/OrderService.Application.csproj", "services/OrderService/src/Application/"]
COPY ["services/OrderService/src/Domain/OrderService.Domain.csproj", "services/OrderService/src/Domain/"]
COPY ["services/OrderService/src/Infrastructure/OrderService.Infrastructure.csproj", "services/OrderService/src/Infrastructure/"]
COPY ["shared/OrderManagement.Shared.Common/OrderManagement.Shared.Common.csproj", "shared/OrderManagement.Shared.Common/"]
COPY ["shared/OrderManagement.Shared.Events/OrderManagement.Shared.Events.csproj", "shared/OrderManagement.Shared.Events/"]
COPY ["shared/OrderManagement.Shared.Security/OrderManagement.Shared.Security.csproj", "shared/OrderManagement.Shared.Security/"]

RUN dotnet restore "services/OrderService/src/Api/OrderService.Api.csproj"

# Copiar todo el código fuente
COPY . .

# Compilar la aplicación
WORKDIR "/src/services/OrderService/src/Api"
RUN dotnet build "OrderService.Api.csproj" -c Release -o /app/build

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish "OrderService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Crear directorio para logs
RUN mkdir -p /app/logs

# Variables de entorno
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5001

# Punto de entrada
ENTRYPOINT ["dotnet", "OrderService.Api.dll"]
