namespace CustomerService.Domain.Repositories;

/// <summary>
/// Unidad de trabajo para manejar transacciones
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Repositorio de clientes
    /// </summary>
    ICustomerRepository Customers { get; }

    /// <summary>
    /// Repositorio de direcciones de clientes
    /// </summary>
    ICustomerAddressRepository CustomerAddresses { get; }

    /// <summary>
    /// Guarda todos los cambios en la base de datos
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Inicia una transacción
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirma la transacción actual
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Revierte la transacción actual
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}