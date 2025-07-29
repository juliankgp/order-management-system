using CustomerService.Domain.Entities;
using CustomerService.Domain.Repositories;
using CustomerService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de direcciones de clientes
/// </summary>
public class CustomerAddressRepository : ICustomerAddressRepository
{
    private readonly CustomerDbContext _context;

    public CustomerAddressRepository(CustomerDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerAddress?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CustomerAddresses
            .Include(a => a.Customer)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<CustomerAddress>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.CustomerAddresses
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Type)
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerAddress?> GetDefaultAddressAsync(Guid customerId, AddressType? type = null, CancellationToken cancellationToken = default)
    {
        var query = _context.CustomerAddresses
            .Where(a => a.CustomerId == customerId && a.IsDefault);

        if (type.HasValue)
        {
            query = query.Where(a => a.Type == type.Value || a.Type == AddressType.Both);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(CustomerAddress address, CancellationToken cancellationToken = default)
    {
        await _context.CustomerAddresses.AddAsync(address, cancellationToken);
    }

    public async Task UpdateAsync(CustomerAddress address, CancellationToken cancellationToken = default)
    {
        _context.CustomerAddresses.Update(address);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var address = await _context.CustomerAddresses.FindAsync([id], cancellationToken);
        if (address != null)
        {
            _context.CustomerAddresses.Remove(address);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CustomerAddresses.AnyAsync(a => a.Id == id, cancellationToken);
    }

    public async Task SetAsDefaultAsync(Guid addressId, Guid customerId, CancellationToken cancellationToken = default)
    {
        // Primero, quitar el flag de default de todas las direcciones del cliente
        var customerAddresses = await _context.CustomerAddresses
            .Where(a => a.CustomerId == customerId)
            .ToListAsync(cancellationToken);

        foreach (var address in customerAddresses)
        {
            address.IsDefault = address.Id == addressId;
            address.UpdatedAt = DateTime.UtcNow;
        }
    }
}