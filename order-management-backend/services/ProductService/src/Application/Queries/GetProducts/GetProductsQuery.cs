using MediatR;
using OrderManagement.Shared.Common.Models;
using ProductService.Application.DTOs;

namespace ProductService.Application.Queries.GetProducts;

/// <summary>
/// Query para obtener productos con paginación y filtros
/// </summary>
public class GetProductsQuery : IRequest<PagedResult<ProductDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Category { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? LowStock { get; set; }
}