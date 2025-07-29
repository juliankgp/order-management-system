using OrderManagement.Shared.Common.Models;

namespace CustomerService.Domain.Entities;

/// <summary>
/// Entidad que representa un cliente en el sistema
/// </summary>
public class Customer : BaseEntity
{
    /// <summary>
    /// Email del cliente (único)
    /// </summary>
    public required string Email { get; set; }
    
    /// <summary>
    /// Hash de la contraseña
    /// </summary>
    public required string PasswordHash { get; set; }
    
    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public required string FirstName { get; set; }
    
    /// <summary>
    /// Apellido del cliente
    /// </summary>
    public required string LastName { get; set; }
    
    /// <summary>
    /// Nombre completo (computed)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
    
    /// <summary>
    /// Número de teléfono
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Fecha de nacimiento
    /// </summary>
    public DateTime? DateOfBirth { get; set; }
    
    /// <summary>
    /// Género
    /// </summary>
    public Gender? Gender { get; set; }
    
    /// <summary>
    /// Indica si el cliente está activo
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Indica si el email ha sido verificado
    /// </summary>
    public bool EmailVerified { get; set; } = false;
    
    /// <summary>
    /// Fecha de verificación del email
    /// </summary>
    public DateTime? EmailVerifiedAt { get; set; }
    
    /// <summary>
    /// Token de verificación de email
    /// </summary>
    public string? EmailVerificationToken { get; set; }
    
    /// <summary>
    /// Fecha de última conexión
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// Preferencias del cliente
    /// </summary>
    public string? Preferences { get; set; }
    
    /// <summary>
    /// Notas internas sobre el cliente
    /// </summary>
    public string? InternalNotes { get; set; }
    
    /// <summary>
    /// Direcciones del cliente
    /// </summary>
    public virtual ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
}

/// <summary>
/// Géneros disponibles
/// </summary>
public enum Gender
{
    Male = 1,
    Female = 2,
    Other = 3,
    PreferNotToSay = 4
}
