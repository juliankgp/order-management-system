using CustomerService.Domain.Entities;

namespace CustomerService.Application.DTOs;

/// <summary>
/// DTO para registro de nuevos clientes
/// </summary>
public class RegisterCustomerDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
}

/// <summary>
/// DTO para login de clientes
/// </summary>
public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO para actualización de perfil de cliente
/// </summary>
public class UpdateCustomerProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? Preferences { get; set; }
}

/// <summary>
/// DTO para cambio de contraseña
/// </summary>
public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// DTO para crear/actualizar direcciones
/// </summary>
public class CreateUpdateAddressDto
{
    public AddressType Type { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string? DeliveryInstructions { get; set; }
}