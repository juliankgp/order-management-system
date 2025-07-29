using CustomerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Infrastructure.Data;

/// <summary>
/// Contexto de base de datos para el servicio de clientes
/// </summary>
public class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerAddress> CustomerAddresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de Customer
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Customers_Email");
            
            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20);
            
            entity.Property(e => e.Preferences)
                .HasMaxLength(1000);
            
            entity.Property(e => e.InternalNotes)
                .HasMaxLength(2000);
            
            entity.Property(e => e.EmailVerificationToken)
                .HasMaxLength(255);

            entity.Property(e => e.Gender)
                .HasConversion<int>();

            entity.Property(e => e.CreatedAt)
                .IsRequired();
            
            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            // Relación con direcciones
            entity.HasMany(e => e.Addresses)
                .WithOne(a => a.Customer)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de CustomerAddress
        modelBuilder.Entity<CustomerAddress>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.AddressLine1)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.AddressLine2)
                .HasMaxLength(255);
            
            entity.Property(e => e.City)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.State)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.ZipCode)
                .IsRequired()
                .HasMaxLength(20);
            
            entity.Property(e => e.Country)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.DeliveryInstructions)
                .HasMaxLength(500);

            entity.Property(e => e.Type)
                .HasConversion<int>();

            entity.Property(e => e.CreatedAt)
                .IsRequired();
            
            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            // Índices
            entity.HasIndex(e => e.CustomerId)
                .HasDatabaseName("IX_CustomerAddresses_CustomerId");
            
            entity.HasIndex(e => new { e.CustomerId, e.IsDefault })
                .HasDatabaseName("IX_CustomerAddresses_CustomerId_IsDefault");
        });

        // Datos de ejemplo
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var customer1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var customer2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var customer3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

        // Clientes de ejemplo
        modelBuilder.Entity<Customer>().HasData(
            new Customer
            {
                Id = customer1Id,
                Email = "john.doe@example.com",
                PasswordHash = "$2a$11$Q7ZXxrfRf1qtj8Zrs9X9nOd7KQx.uxF8UqWJGbE9JN6yLzfxjMNqe", // Password123!
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1-555-0123",
                DateOfBirth = new DateTime(1985, 6, 15),
                Gender = Gender.Male,
                IsActive = true,
                EmailVerified = true,
                EmailVerifiedAt = DateTime.UtcNow.AddDays(-30),
                LastLoginAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Customer
            {
                Id = customer2Id,
                Email = "jane.smith@example.com",
                PasswordHash = "$2a$11$Q7ZXxrfRf1qtj8Zrs9X9nOd7KQx.uxF8UqWJGbE9JN6yLzfxjMNqe", // Password123!
                FirstName = "Jane",
                LastName = "Smith",
                PhoneNumber = "+1-555-0456",
                DateOfBirth = new DateTime(1990, 3, 22),
                Gender = Gender.Female,
                IsActive = true,
                EmailVerified = true,
                EmailVerifiedAt = DateTime.UtcNow.AddDays(-25),
                LastLoginAt = DateTime.UtcNow.AddHours(-2),
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                UpdatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new Customer
            {
                Id = customer3Id,
                Email = "alex.johnson@example.com",
                PasswordHash = "$2a$11$Q7ZXxrfRf1qtj8Zrs9X9nOd7KQx.uxF8UqWJGbE9JN6yLzfxjMNqe", // Password123!
                FirstName = "Alex",
                LastName = "Johnson",
                PhoneNumber = "+1-555-0789",
                Gender = Gender.Other,
                IsActive = true,
                EmailVerified = false,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            }
        );

        // Direcciones de ejemplo
        modelBuilder.Entity<CustomerAddress>().HasData(
            new CustomerAddress
            {
                Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
                CustomerId = customer1Id,
                Type = AddressType.Both,
                AddressLine1 = "123 Main Street",
                AddressLine2 = "Apt 4B",
                City = "New York",
                State = "NY",
                ZipCode = "10001",
                Country = "USA",
                IsDefault = true,
                DeliveryInstructions = "Leave at front door",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new CustomerAddress
            {
                Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"),
                CustomerId = customer2Id,
                Type = AddressType.Shipping,
                AddressLine1 = "456 Oak Avenue",
                City = "Los Angeles",
                State = "CA",
                ZipCode = "90210",
                Country = "USA",
                IsDefault = true,
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                UpdatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new CustomerAddress
            {
                Id = Guid.Parse("a2222223-2222-2222-2222-222222222222"),
                CustomerId = customer2Id,
                Type = AddressType.Billing,
                AddressLine1 = "789 Business Blvd",
                AddressLine2 = "Suite 100",
                City = "Los Angeles",
                State = "CA",
                ZipCode = "90211",
                Country = "USA",
                IsDefault = false,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-20)
            }
        );
    }
}