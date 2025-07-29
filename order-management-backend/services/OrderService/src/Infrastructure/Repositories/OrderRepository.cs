using Microsoft.EntityFrameworkCore;
using OrderManagement.Shared.Common.Models;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Data;

namespace OrderService.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de órdenes
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => !o.IsDeleted)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => !o.IsDeleted)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => !o.IsDeleted)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<PagedResult<Order>> GetPagedAsync(
        int page, 
        int pageSize, 
        Guid? customerId = null, 
        string? status = null, 
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        string? orderNumber = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .Where(o => !o.IsDeleted);

        // Aplicar filtros
        if (customerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == customerId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            {
                query = query.Where(o => o.Status == orderStatus);
            }
        }

        if (fromDate.HasValue)
        {
            query = query.Where(o => o.OrderDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(o => o.OrderDate <= toDate.Value);
        }

        if (!string.IsNullOrEmpty(orderNumber))
        {
            query = query.Where(o => o.OrderNumber.Contains(orderNumber));
        }

        // Contar total de registros
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplicar paginación
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Order>
        {
            Items = orders,
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<Order>> GetByCustomerIdAsync(Guid customerId, PaginationParameters parameters, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId && !o.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Order>
        {
            Items = orders,
            TotalCount = totalCount,
            CurrentPage = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<int> CountTodayOrdersAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await _context.Orders
            .Where(o => o.CreatedAt >= today && o.CreatedAt < tomorrow)
            .CountAsync(cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
    }

    public void Update(Order order)
    {
        _context.Orders.Update(order);
    }

    public void Delete(Order order)
    {
        order.IsDeleted = true;
        order.DeletedAt = DateTime.UtcNow;
        _context.Orders.Update(order);
    }
}
