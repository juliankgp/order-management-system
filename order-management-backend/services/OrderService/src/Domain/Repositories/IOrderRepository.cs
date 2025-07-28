using OrderManagement.Shared.Common.Models;
using OrderService.Domain.Entities;

namespace OrderService.Domain.Repositories;

/// <summary>
/// Repositorio para el manejo de órdenes
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Obtiene una orden por su ID
    /// </summary>
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene una orden por su número
    /// </summary>
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene todas las órdenes con paginación
    /// </summary>
    Task<PagedResult<Order>> GetPagedAsync(PaginationParameters parameters, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene órdenes por cliente ID
    /// </summary>
    Task<PagedResult<Order>> GetByCustomerIdAsync(Guid customerId, PaginationParameters parameters, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene órdenes por estado
    /// </summary>
    Task<PagedResult<Order>> GetByStatusAsync(OrderStatus status, PaginationParameters parameters, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Agrega una nueva orden
    /// </summary>
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Actualiza una orden existente
    /// </summary>
    Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Elimina una orden (soft delete)
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si existe una orden con el número especificado
    /// </summary>
    Task<bool> ExistsAsync(string orderNumber, CancellationToken cancellationToken = default);
}
