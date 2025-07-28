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
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<PagedResult<Order>> GetPagedAsync(PaginationParameters parameters, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .AsQueryable();

        // Aplicar filtro de búsqueda
        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            query = query.Where(o => o.OrderNumber.Contains(parameters.SearchTerm) ||
                                   o.ShippingCity.Contains(parameters.SearchTerm) ||
                                   o.ShippingCountry.Contains(parameters.SearchTerm));
        }

        // Aplicar ordenamiento
        query = parameters.SortBy?.ToLower() switch
        {
            "ordernumber" => parameters.SortDirection.ToLower() == "desc" 
                ? query.OrderByDescending(o => o.OrderNumber)
                : query.OrderBy(o => o.OrderNumber),
            "orderdate" => parameters.SortDirection.ToLower() == "desc" 
                ? query.OrderByDescending(o => o.OrderDate)
                : query.OrderBy(o => o.OrderDate),
            "totalamount" => parameters.SortDirection.ToLower() == "desc" 
                ? query.OrderByDescending(o => o.TotalAmount)
                : query.OrderBy(o => o.TotalAmount),
            "status" => parameters.SortDirection.ToLower() == "desc" 
                ? query.OrderByDescending(o => o.Status)
                : query.OrderBy(o => o.Status),
            _ => query.OrderByDescending(o => o.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Order>
        {
            Items = items,
            TotalCount = totalCount,
            CurrentPage = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<PagedResult<Order>> GetByCustomerIdAsync(Guid customerId, PaginationParameters parameters, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId);

        // Aplicar filtro de búsqueda
        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            query = query.Where(o => o.OrderNumber.Contains(parameters.SearchTerm));
        }

        // Aplicar ordenamiento (por defecto por fecha descendente)
        query = parameters.SortBy?.ToLower() switch
        {
            "ordernumber" => parameters.SortDirection.ToLower() == "desc" 
                ? query.OrderByDescending(o => o.OrderNumber)
                : query.OrderBy(o => o.OrderNumber),
            "totalamount" => parameters.SortDirection.ToLower() == "desc" 
                ? query.OrderByDescending(o => o.TotalAmount)
                : query.OrderBy(o => o.TotalAmount),
            _ => query.OrderByDescending(o => o.OrderDate)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Order>
        {
            Items = items,
            TotalCount = totalCount,
            CurrentPage = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<PagedResult<Order>> GetByStatusAsync(OrderStatus status, PaginationParameters parameters, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .Where(o => o.Status == status);

        // Aplicar filtro de búsqueda
        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            query = query.Where(o => o.OrderNumber.Contains(parameters.SearchTerm) ||
                                   o.ShippingCity.Contains(parameters.SearchTerm));
        }

        // Aplicar ordenamiento
        query = parameters.SortBy?.ToLower() switch
        {
            "ordernumber" => parameters.SortDirection.ToLower() == "desc" 
                ? query.OrderByDescending(o => o.OrderNumber)
                : query.OrderBy(o => o.OrderNumber),
            "orderdate" => parameters.SortDirection.ToLower() == "desc" 
                ? query.OrderByDescending(o => o.OrderDate)
                : query.OrderBy(o => o.OrderDate),
            _ => query.OrderByDescending(o => o.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Order>
        {
            Items = items,
            TotalCount = totalCount,
            CurrentPage = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Add(order);
        return order;
    }

    public async Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(order);
        return order;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FindAsync(new object[] { id }, cancellationToken);
        if (order != null)
        {
            order.IsDeleted = true;
            order.DeletedAt = DateTime.UtcNow;
            _context.Orders.Update(order);
        }
    }

    public async Task<bool> ExistsAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AnyAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }
}
