using Microsoft.EntityFrameworkCore;
using OrderManagement.Shared.Common.Models;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de productos
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.StockMovements)
            .Include(p => p.PriceHistory)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.StockMovements)
            .Include(p => p.PriceHistory)
            .FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);
    }

    public async Task<PagedResult<Product>> GetPagedAsync(int page, int pageSize, string? category = null, string? searchTerm = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Products.AsQueryable();

        // Filtros
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category.ToLower().Contains(category.ToLower()));
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()) ||
                                   p.Description!.ToLower().Contains(searchTerm.ToLower()) ||
                                   p.Sku.ToLower().Contains(searchTerm.ToLower()) ||
                                   p.Tags!.ToLower().Contains(searchTerm.ToLower()));
        }

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalItems,
            CurrentPage = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(p => p.Category.ToLower() == category.ToLower() && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(p => p.Stock <= p.MinimumStock && p.IsActive)
            .OrderBy(p => p.Stock)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FindAsync([id], cancellationToken);
        if (product != null)
        {
            _context.Products.Remove(product);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<bool> HasSufficientStockAsync(Guid productId, int requiredQuantity, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FindAsync([productId], cancellationToken);
        return product != null && product.Stock >= requiredQuantity;
    }

    public async Task UpdateStockAsync(Guid productId, int newStock, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FindAsync([productId], cancellationToken);
        if (product != null)
        {
            product.Stock = newStock;
            product.UpdatedAt = DateTime.UtcNow;
        }
    }
}