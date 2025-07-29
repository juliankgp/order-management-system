using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Data;

/// <summary>
/// Contexto de base de datos para el servicio de productos
/// </summary>
public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }
    public DbSet<PriceHistory> PriceHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Sku).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Brand).HasMaxLength(100);
            entity.Property(e => e.Dimensions).HasMaxLength(50);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Weight).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Índices
            entity.HasIndex(e => e.Sku).IsUnique();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Stock);
        });

        // Configuración de StockMovement
        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ExternalReference).HasMaxLength(100);
            entity.Property(e => e.Comments).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Relación con Product
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.StockMovements)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Índices
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.MovementType);
        });

        // Configuración de PriceHistory
        modelBuilder.Entity<PriceHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PreviousPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.NewPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Reason).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Relación con Product
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.PriceHistory)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Índices
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.EffectiveDate);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var products = new[]
        {
            new Product
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Laptop Dell XPS 13",
                Description = "Laptop ultrabook con procesador Intel Core i7, 16GB RAM, 512GB SSD",
                Sku = "DELL-XPS13-001",
                Price = 1299.99m,
                Stock = 25,
                MinimumStock = 5,
                Category = "Electronics",
                Brand = "Dell",
                Weight = 1200,
                Dimensions = "30.2 x 19.9 x 1.1 cm",
                IsActive = true,
                Tags = "laptop, computer, dell, ultrabook",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "iPhone 15 Pro",
                Description = "Smartphone Apple iPhone 15 Pro 256GB, Titanio Natural",
                Sku = "APPLE-IP15P-256",
                Price = 1199.99m,
                Stock = 15,
                MinimumStock = 3,
                Category = "Electronics",
                Brand = "Apple",
                Weight = 187,
                Dimensions = "14.67 x 7.08 x 0.81 cm",
                IsActive = true,
                Tags = "smartphone, iphone, apple, mobile",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Mesa de Oficina",
                Description = "Mesa de oficina moderna con superficie de cristal templado",
                Sku = "FURNITURE-DESK-001",
                Price = 299.99m,
                Stock = 8,
                MinimumStock = 2,
                Category = "Furniture",
                Brand = "OfficeMax",
                Weight = 25000,
                Dimensions = "140 x 70 x 75 cm",
                IsActive = true,
                Tags = "desk, office, furniture, glass",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        modelBuilder.Entity<Product>().HasData(products);
    }
}