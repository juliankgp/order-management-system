using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagement.Shared.Common.Models;
using ProductService.Application.DTOs;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Queries.GetProducts;

/// <summary>
/// Handler para obtener productos con paginación y filtros
/// </summary>
public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProductsQueryHandler> _logger;

    public GetProductsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetProductsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting products - Page: {Page}, PageSize: {PageSize}, Category: {Category}, SearchTerm: {SearchTerm}",
            request.Page, request.PageSize, request.Category, request.SearchTerm);

        // Validar parámetros
        if (request.Page < 1) request.Page = 1;
        if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 10;

        var result = await _unitOfWork.Products.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Category,
            request.SearchTerm,
            request.IsActive,
            cancellationToken);

        var productDtos = _mapper.Map<List<ProductDto>>(result.Items);

        var pagedResult = new PagedResult<ProductDto>
        {
            Items = productDtos,
            TotalCount = result.TotalCount,
            CurrentPage = result.CurrentPage,
            PageSize = result.PageSize
        };

        _logger.LogInformation("Retrieved {Count} products from {TotalCount} total", 
            productDtos.Count, result.TotalCount);

        return pagedResult;
    }
}