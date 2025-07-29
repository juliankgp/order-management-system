using CustomerService.Application.DTOs;
using MediatR;

namespace CustomerService.Application.Commands.LoginCustomer;

/// <summary>
/// Comando para autenticar un cliente
/// </summary>
public record LoginCustomerCommand(
    string Email,
    string Password
) : IRequest<AuthenticatedCustomerDto>;