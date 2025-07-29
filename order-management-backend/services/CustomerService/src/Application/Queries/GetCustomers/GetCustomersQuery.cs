using CustomerService.Application.DTOs;
using MediatR;
using OrderManagement.Shared.Common.Models;

namespace CustomerService.Application.Queries.GetCustomers;

/// <summary>
/// Query para obtener clientes con paginación
/// </summary>
public record GetCustomersQuery(
    int Page = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsActive = null
) : IRequest<PagedResult<CustomerSummaryDto>>;