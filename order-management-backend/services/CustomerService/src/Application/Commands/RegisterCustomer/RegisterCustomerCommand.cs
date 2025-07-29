using CustomerService.Application.DTOs;
using CustomerService.Domain.Entities;
using MediatR;

namespace CustomerService.Application.Commands.RegisterCustomer;

/// <summary>
/// Comando para registrar un nuevo cliente
/// </summary>
public record RegisterCustomerCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber = null,
    DateTime? DateOfBirth = null,
    Gender? Gender = null
) : IRequest<AuthenticatedCustomerDto>;