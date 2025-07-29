using OrderManagement.Shared.Common.Models;

namespace ProductService.Domain.Entities;

/// <summary>
/// Entidad que representa un producto en el sistema
/// </summary>
public class Product : BaseEntity
{
    /// <summary>
    /// Nombre del producto
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Código SKU único del producto
    /// </summary>
    public required string Sku { get; set; }
    
    /// <summary>
    /// Descripción del producto
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Precio del producto
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Stock disponible
    /// </summary>
    public int Stock { get; set; }
    
    /// <summary>
    /// Stock mínimo para alertas
    /// </summary>
    public int MinimumStock { get; set; }
    
    /// <summary>
    /// Categoría del producto
    /// </summary>
    public required string Category { get; set; }
    
    /// <summary>
    /// Marca del producto
    /// </summary>
    public string? Brand { get; set; }
    
    /// <summary>
    /// Peso del producto en gramos
    /// </summary>
    public decimal? Weight { get; set; }
    
    /// <summary>
    /// Dimensiones del producto (LxWxH en cm)
    /// </summary>
    public string? Dimensions { get; set; }
    
    /// <summary>
    /// Indica si el producto está activo
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// URL de la imagen principal del producto
    /// </summary>
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// Tags del producto para búsqueda
    /// </summary>
    public string? Tags { get; set; }
    
    /// <summary>
    /// Historial de movimientos de stock
    /// </summary>
    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    
    /// <summary>
    /// Historial de cambios de precio
    /// </summary>
    public virtual ICollection<PriceHistory> PriceHistory { get; set; } = new List<PriceHistory>();
}

/// <summary>
/// Tipos de movimiento de stock
/// </summary>
public enum StockMovementType
{
    /// <summary>
    /// Entrada de stock (compra, devolución, ajuste positivo)
    /// </summary>
    In = 1,
    
    /// <summary>
    /// Salida de stock (venta, ajuste negativo, pérdida)
    /// </summary>
    Out = 2,
    
    /// <summary>
    /// Reserva de stock (para órdenes pendientes)
    /// </summary>
    Reserved = 3,
    
    /// <summary>
    /// Liberación de reserva
    /// </summary>
    Released = 4
}
