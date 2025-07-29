using CustomerService.Application.DTOs;
using CustomerService.Domain.Entities;
using MediatR;

namespace CustomerService.Application.Commands.UpdateCustomerProfile;

/// <summary>
/// Comando para actualizar el perfil de un cliente
/// </summary>
public record UpdateCustomerProfileCommand(
    Guid CustomerId,
    string FirstName,
    string LastName,
    string? PhoneNumber = null,
    DateTime? DateOfBirth = null,
    Gender? Gender = null,
    string? Preferences = null
) : IRequest<CustomerDto>;