using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de movimientos de stock
/// </summary>
public class StockMovementRepository : IStockMovementRepository
{
    private readonly ProductDbContext _context;

    public StockMovementRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<StockMovement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.StockMovements
            .Include(sm => sm.Product)
            .FirstOrDefaultAsync(sm => sm.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<StockMovement>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.StockMovements
            .Include(sm => sm.Product)
            .Where(sm => sm.ProductId == productId)
            .OrderByDescending(sm => sm.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StockMovement>> GetByProductIdPagedAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.StockMovements
            .Include(sm => sm.Product)
            .Where(sm => sm.ProductId == productId)
            .OrderByDescending(sm => sm.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StockMovement>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.StockMovements
            .Include(sm => sm.Product)
            .Where(sm => sm.CreatedAt >= from && sm.CreatedAt <= to)
            .OrderByDescending(sm => sm.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(StockMovement stockMovement, CancellationToken cancellationToken = default)
    {
        await _context.StockMovements.AddAsync(stockMovement, cancellationToken);
    }

    public async Task<decimal> GetCurrentStockAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FindAsync([productId], cancellationToken);
        return product?.Stock ?? 0;
    }
}