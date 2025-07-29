using CustomerService.Domain.Entities;

namespace CustomerService.Domain.Repositories;

/// <summary>
/// Repositorio para el manejo de direcciones de clientes
/// </summary>
public interface ICustomerAddressRepository
{
    /// <summary>
    /// Obtiene una dirección por su ID
    /// </summary>
    Task<CustomerAddress?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las direcciones de un cliente
    /// </summary>
    Task<IEnumerable<CustomerAddress>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la dirección por defecto de un cliente
    /// </summary>
    Task<CustomerAddress?> GetDefaultAddressAsync(Guid customerId, AddressType? type = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega una nueva dirección
    /// </summary>
    Task AddAsync(CustomerAddress address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza una dirección existente
    /// </summary>
    Task UpdateAsync(CustomerAddress address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina una dirección
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe una dirección por ID
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Establece una dirección como predeterminada
    /// </summary>
    Task SetAsDefaultAsync(Guid addressId, Guid customerId, CancellationToken cancellationToken = default);
}