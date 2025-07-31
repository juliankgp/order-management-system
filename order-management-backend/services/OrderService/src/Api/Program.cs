using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Shared.Security.Models;
using OrderManagement.Shared.Security.Services;
using OrderManagement.Shared.Security.Extensions;
using OrderService.Application.Mappings;
using OrderService.Application.Interfaces;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.ExternalServices;
using OrderService.Infrastructure.Repositories;
using OrderService.Api.Middleware;
using Serilog;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "OrderService")
    .WriteTo.Console()
    .WriteTo.File("logs/orderservice-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Order Service API", Version = "v1" });
    
    // Configurar JWT en Swagger
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Entity Framework
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.RegisterServicesFromAssembly(typeof(OrderService.Application.Commands.CreateOrder.CreateOrderCommand).Assembly);
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(OrderService.Application.Commands.CreateOrder.CreateOrderCommand).Assembly);

// AutoMapper
builder.Services.AddAutoMapper(typeof(OrderMappingProfile));

// Repositorios y Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// External Services con JWT automático
builder.Services.AddHttpContextAccessor(); // Necesario para acceder al HttpContext en el DelegatingHandler

builder.Services.AddHttpClientWithJwt<OrderService.Application.Interfaces.IProductService, ProductService>(client =>
{
    var baseUrl = builder.Configuration["ExternalServices:ProductService:BaseUrl"] ?? "http://localhost:5002";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClientWithJwt<OrderService.Application.Interfaces.ICustomerService, CustomerService>(client =>
{
    var baseUrl = builder.Configuration["ExternalServices:CustomerService:BaseUrl"] ?? "http://localhost:5003";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Event Bus Service
builder.Services.AddSingleton<OrderService.Application.Interfaces.IEventBusService, RabbitMQEventBusService>();

// JWT Authentication (Nueva implementación unificada)
builder.Services.AddJwtAuthentication(builder.Configuration);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service API V1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz
    });
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Middleware de debug JWT (solo para desarrollo)
if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<JwtDebugMiddleware>();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    try
    {
        context.Database.EnsureCreated();
        Log.Information("Database ensured for OrderService");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while ensuring the database for OrderService");
    }
}

Log.Information("OrderService is starting up on port 5001");

app.Run();
