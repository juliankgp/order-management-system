using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Shared.Security.Models;
using OrderManagement.Shared.Security.Services;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Repositories;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
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

// Repositorios y Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// JWT Configuration
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IJwtService, JwtService>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
if (jwtSettings != null)
{
    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            options.Authority = jwtSettings.Issuer;
            options.Audience = jwtSettings.Audience;
            options.RequireHttpsMetadata = false; // Solo para desarrollo
        });
}

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
    .AddDbContext<OrderDbContext>();

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
