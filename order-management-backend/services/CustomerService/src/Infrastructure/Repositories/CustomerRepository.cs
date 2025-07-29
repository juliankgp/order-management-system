using CustomerService.Domain.Entities;
using CustomerService.Domain.Repositories;
using CustomerService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Shared.Common.Models;

namespace CustomerService.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de clientes
/// </summary>
public class CustomerRepository : ICustomerRepository
{
    private readonly CustomerDbContext _context;

    public CustomerRepository(CustomerDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<PagedResult<Customer>> GetPagedAsync(int page, int pageSize, string? searchTerm = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.AsQueryable();

        // Filtros
        if (!string.IsNullOrEmpty(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLowerInvariant();
            query = query.Where(c => 
                c.Email.ToLower().Contains(lowerSearchTerm) ||
                c.FirstName.ToLower().Contains(lowerSearchTerm) ||
                c.LastName.ToLower().Contains(lowerSearchTerm) ||
                (c.PhoneNumber != null && c.PhoneNumber.Contains(searchTerm)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Customer>
        {
            Items = items,
            TotalCount = totalItems,
            CurrentPage = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<Customer>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Where(c => ids.Contains(c.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
    }

    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _context.Customers.Update(customer);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FindAsync([id], cancellationToken);
        if (customer != null)
        {
            _context.Customers.Remove(customer);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.AnyAsync(c => c.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task UpdateLastLoginAsync(Guid customerId, DateTime lastLogin, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FindAsync([customerId], cancellationToken);
        if (customer != null)
        {
            customer.LastLoginAt = lastLogin;
            customer.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task VerifyEmailAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FindAsync([customerId], cancellationToken);
        if (customer != null)
        {
            customer.EmailVerified = true;
            customer.EmailVerifiedAt = DateTime.UtcNow;
            customer.EmailVerificationToken = null;
            customer.UpdatedAt = DateTime.UtcNow;
        }
    }
}