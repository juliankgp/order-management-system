using CustomerService.Application.DTOs;
using MediatR;

namespace CustomerService.Application.Queries.GetCustomer;

/// <summary>
/// Query para obtener un cliente por ID
/// </summary>
public record GetCustomerQuery(Guid CustomerId) : IRequest<CustomerDto?>;