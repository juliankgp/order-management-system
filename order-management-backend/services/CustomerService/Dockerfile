# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["shared/OrderManagement.Shared.Common/OrderManagement.Shared.Common.csproj", "shared/OrderManagement.Shared.Common/"]
COPY ["shared/OrderManagement.Shared.Events/OrderManagement.Shared.Events.csproj", "shared/OrderManagement.Shared.Events/"]
COPY ["shared/OrderManagement.Shared.Security/OrderManagement.Shared.Security.csproj", "shared/OrderManagement.Shared.Security/"]
COPY ["services/CustomerService/src/Domain/CustomerService.Domain.csproj", "services/CustomerService/src/Domain/"]
COPY ["services/CustomerService/src/Application/CustomerService.Application.csproj", "services/CustomerService/src/Application/"]
COPY ["services/CustomerService/src/Infrastructure/CustomerService.Infrastructure.csproj", "services/CustomerService/src/Infrastructure/"]
COPY ["services/CustomerService/src/Api/CustomerService.Api.csproj", "services/CustomerService/src/Api/"]

RUN dotnet restore "services/CustomerService/src/Api/CustomerService.Api.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/services/CustomerService/src/Api"
RUN dotnet build "CustomerService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "CustomerService.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create logs directory
RUN mkdir -p logs

COPY --from=publish /app/publish .

EXPOSE 5003

ENV ASPNETCORE_URLS=http://*:5003
ENV ASPNETCORE_ENVIRONMENT=Docker

ENTRYPOINT ["dotnet", "CustomerService.Api.dll"]