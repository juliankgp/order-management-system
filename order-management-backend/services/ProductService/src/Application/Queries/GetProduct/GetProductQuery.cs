using MediatR;
using ProductService.Application.DTOs;

namespace ProductService.Application.Queries.GetProduct;

/// <summary>
/// Query para obtener un producto por ID
/// </summary>
public record GetProductQuery(Guid Id) : IRequest<ProductDto?>;