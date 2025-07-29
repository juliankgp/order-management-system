using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Data;

/// <summary>
/// Contexto de base de datos para OrderService
/// </summary>
public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Órdenes
    /// </summary>
    public DbSet<Order> Orders => Set<Order>();
    
    /// <summary>
    /// Items de órdenes
    /// </summary>
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    
    /// <summary>
    /// Historial de estado de órdenes
    /// </summary>
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.SubTotal)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.TaxAmount)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.ShippingCost)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.ShippingAddress)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.ShippingCity)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ShippingZipCode)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.ShippingCountry)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Notes)
                .HasMaxLength(500);

            // Índices
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.OrderDate);

            // Relaciones
            entity.HasMany(e => e.Items)
                .WithOne(e => e.Order)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.StatusHistory)
                .WithOne(e => e.Order)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Query filter para soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configuración de OrderItem
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ProductName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ProductSku)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.Discount)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.Notes)
                .HasMaxLength(500);

            // Índices
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.ProductId);

            // Query filter para soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configuración de OrderStatusHistory
        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.ToTable("OrderStatusHistories");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Reason)
                .HasMaxLength(200);

            entity.Property(e => e.Comments)
                .HasMaxLength(500);

            // Índices
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.ChangedAt);

            // Query filter para soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Actualizar timestamps automáticamente
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is OrderManagement.Shared.Common.Models.BaseEntity && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (OrderManagement.Shared.Common.Models.BaseEntity)entityEntry.Entity;

            if (entityEntry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }

            entity.UpdatedAt = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
