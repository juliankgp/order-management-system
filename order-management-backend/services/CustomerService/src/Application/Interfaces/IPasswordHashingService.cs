namespace CustomerService.Application.Interfaces;

/// <summary>
/// Servicio para el manejo de hash de contraseñas
/// </summary>
public interface IPasswordHashingService
{
    /// <summary>
    /// Genera un hash para la contraseña
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verifica si una contraseña coincide con el hash
    /// </summary>
    bool VerifyPassword(string password, string hashedPassword);
}