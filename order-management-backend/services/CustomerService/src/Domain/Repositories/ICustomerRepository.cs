using CustomerService.Domain.Entities;
using OrderManagement.Shared.Common.Models;

namespace CustomerService.Domain.Repositories;

/// <summary>
/// Repositorio para el manejo de clientes
/// </summary>
public interface ICustomerRepository
{
    /// <summary>
    /// Obtiene un cliente por su ID
    /// </summary>
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un cliente por su email
    /// </summary>
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene clientes con paginación
    /// </summary>
    Task<PagedResult<Customer>> GetPagedAsync(int page, int pageSize, string? searchTerm = null, bool? isActive = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene clientes por IDs
    /// </summary>
    Task<IEnumerable<Customer>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega un nuevo cliente
    /// </summary>
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un cliente existente
    /// </summary>
    Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un cliente
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe un cliente por ID
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe un cliente por email
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza la fecha de última conexión
    /// </summary>
    Task UpdateLastLoginAsync(Guid customerId, DateTime lastLogin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica el email del cliente
    /// </summary>
    Task VerifyEmailAsync(Guid customerId, CancellationToken cancellationToken = default);
}