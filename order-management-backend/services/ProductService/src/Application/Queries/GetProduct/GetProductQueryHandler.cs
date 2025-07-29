using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ProductService.Application.DTOs;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Queries.GetProduct;

/// <summary>
/// Handler para obtener un producto por ID
/// </summary>
public class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProductQueryHandler> _logger;

    public GetProductQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetProductQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductDto?> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting product with ID {ProductId}", request.Id);

        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);
        
        if (product == null)
        {
            _logger.LogWarning("Product {ProductId} not found", request.Id);
            return null;
        }

        var result = _mapper.Map<ProductDto>(product);
        
        _logger.LogInformation("Product {ProductId} retrieved successfully", request.Id);
        return result;
    }
}