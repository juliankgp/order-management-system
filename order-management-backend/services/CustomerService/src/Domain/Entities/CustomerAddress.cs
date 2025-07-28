using OrderManagement.Shared.Common.Models;

namespace CustomerService.Domain.Entities;

/// <summary>
/// Entidad que representa una dirección de cliente
/// </summary>
public class CustomerAddress : BaseEntity
{
    /// <summary>
    /// ID del cliente propietario
    /// </summary>
    public Guid CustomerId { get; set; }
    
    /// <summary>
    /// Navegación al cliente
    /// </summary>
    public virtual Customer Customer { get; set; } = null!;
    
    /// <summary>
    /// Tipo de dirección
    /// </summary>
    public AddressType Type { get; set; }
    
    /// <summary>
    /// Línea 1 de la dirección
    /// </summary>
    public required string AddressLine1 { get; set; }
    
    /// <summary>
    /// Línea 2 de la dirección (opcional)
    /// </summary>
    public string? AddressLine2 { get; set; }
    
    /// <summary>
    /// Ciudad
    /// </summary>
    public required string City { get; set; }
    
    /// <summary>
    /// Estado o provincia
    /// </summary>
    public required string State { get; set; }
    
    /// <summary>
    /// Código postal
    /// </summary>
    public required string ZipCode { get; set; }
    
    /// <summary>
    /// País
    /// </summary>
    public required string Country { get; set; }
    
    /// <summary>
    /// Indica si es la dirección por defecto
    /// </summary>
    public bool IsDefault { get; set; } = false;
    
    /// <summary>
    /// Instrucciones especiales de entrega
    /// </summary>
    public string? DeliveryInstructions { get; set; }
}

/// <summary>
/// Tipos de dirección
/// </summary>
public enum AddressType
{
    /// <summary>
    /// Dirección de facturación
    /// </summary>
    Billing = 1,
    
    /// <summary>
    /// Dirección de envío
    /// </summary>
    Shipping = 2,
    
    /// <summary>
    /// Dirección que sirve para ambos propósitos
    /// </summary>
    Both = 3
}
