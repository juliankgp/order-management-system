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
    /// Obtiene una orden por su ID incluyendo los items
    /// </summary>
    Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene una orden por su número
    /// </summary>
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene todas las órdenes con paginación y filtros
    /// </summary>
    Task<PagedResult<Order>> GetPagedAsync(
        int page, 
        int pageSize, 
        Guid? customerId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? orderNumber = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene órdenes por cliente ID
    /// </summary>
    Task<PagedResult<Order>> GetByCustomerIdAsync(Guid customerId, PaginationParameters parameters, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cuenta las órdenes creadas hoy
    /// </summary>
    Task<int> CountTodayOrdersAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Agrega una nueva orden
    /// </summary>
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Actualiza una orden
    /// </summary>
    void Update(Order order);
    
    /// <summary>
    /// Elimina una orden (soft delete)
    /// </summary>
    void Delete(Order order);
}
