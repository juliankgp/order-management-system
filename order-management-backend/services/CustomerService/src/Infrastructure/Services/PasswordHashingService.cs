using CustomerService.Application.Interfaces;

namespace CustomerService.Infrastructure.Services;

/// <summary>
/// Servicio para el manejo de hash de contraseñas usando BCrypt
/// </summary>
public class PasswordHashingService : IPasswordHashingService
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}