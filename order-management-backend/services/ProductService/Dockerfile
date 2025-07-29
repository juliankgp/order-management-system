# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["shared/OrderManagement.Shared.Common/OrderManagement.Shared.Common.csproj", "shared/OrderManagement.Shared.Common/"]
COPY ["shared/OrderManagement.Shared.Events/OrderManagement.Shared.Events.csproj", "shared/OrderManagement.Shared.Events/"]
COPY ["shared/OrderManagement.Shared.Security/OrderManagement.Shared.Security.csproj", "shared/OrderManagement.Shared.Security/"]
COPY ["services/ProductService/src/Domain/ProductService.Domain.csproj", "services/ProductService/src/Domain/"]
COPY ["services/ProductService/src/Application/ProductService.Application.csproj", "services/ProductService/src/Application/"]
COPY ["services/ProductService/src/Infrastructure/ProductService.Infrastructure.csproj", "services/ProductService/src/Infrastructure/"]
COPY ["services/ProductService/src/Api/ProductService.Api.csproj", "services/ProductService/src/Api/"]

RUN dotnet restore "services/ProductService/src/Api/ProductService.Api.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/services/ProductService/src/Api"
RUN dotnet build "ProductService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ProductService.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create logs directory
RUN mkdir -p logs

COPY --from=publish /app/publish .

EXPOSE 5002

ENV ASPNETCORE_URLS=http://*:5002
ENV ASPNETCORE_ENVIRONMENT=Docker

ENTRYPOINT ["dotnet", "ProductService.Api.dll"]