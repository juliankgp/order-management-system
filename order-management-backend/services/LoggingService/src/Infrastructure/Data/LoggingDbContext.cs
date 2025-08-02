using LoggingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Shared.Common.Models;

namespace LoggingService.Infrastructure.Data;

public class LoggingDbContext : DbContext
{
    public LoggingDbContext(DbContextOptions<LoggingDbContext> options) : base(options)
    {
    }

    public DbSet<LogEntry> LogEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LogEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Level)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(e => e.Message)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(e => e.ServiceName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.CorrelationId)
                .HasMaxLength(100);

            entity.Property(e => e.Exception)
                .HasMaxLength(4000);

            entity.Property(e => e.StackTrace)
                .HasMaxLength(8000);

            entity.Property(e => e.Properties)
                .HasMaxLength(4000);

            entity.Property(e => e.MachineName)
                .HasMaxLength(100);

            entity.Property(e => e.Environment)
                .HasMaxLength(50);

            entity.Property(e => e.ApplicationVersion)
                .HasMaxLength(50);

            entity.Property(e => e.Timestamp)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            // Índices para mejorar performance de consultas
            entity.HasIndex(e => e.ServiceName);
            entity.HasIndex(e => e.Level);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.CorrelationId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => new { e.ServiceName, e.Level, e.Timestamp });
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        
        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
        
        return base.SaveChangesAsync(cancellationToken);
    }
}