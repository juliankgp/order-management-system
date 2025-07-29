using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Shared.Common.Models;
using ProductService.Application.DTOs;
using ProductService.Application.Queries.GetProduct;
using ProductService.Application.Queries.GetProducts;

namespace ProductService.Api.Controllers;

/// <summary>
/// Controlador para el manejo de productos
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Test endpoint to verify service is running
    /// </summary>
    [HttpGet("test")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    public ActionResult<string> Test()
    {
        return Ok("ProductService is running successfully!");
    }

    /// <summary>
    /// Obtiene todos los productos con paginación y filtros
    /// </summary>
    /// <param name="page">Número de página</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <param name="category">Categoría del producto (filtro opcional)</param>
    /// <param name="searchTerm">Término de búsqueda (filtro opcional)</param>
    /// <param name="isActive">Estado activo del producto (filtro opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista paginada de productos</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? category = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validaciones básicas
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            _logger.LogInformation("Getting products - Page: {Page}, PageSize: {PageSize}, Category: {Category}, SearchTerm: {SearchTerm}",
                page, pageSize, category, searchTerm);

            var query = new GetProductsQuery
            {
                Page = page,
                PageSize = pageSize,
                Category = category,
                SearchTerm = searchTerm,
                IsActive = isActive
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products");
            return StatusCode(500, new { message = "Internal server error occurred while retrieving products" });
        }
    }

    /// <summary>
    /// Obtiene un producto por ID
    /// </summary>
    /// <param name="id">ID del producto</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El producto solicitado</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ProductDto>> GetProduct(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting product {ProductId}", id);

            var query = new GetProductQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result == null)
            {
                _logger.LogWarning("Product {ProductId} not found", id);
                return NotFound(new { message = $"Product {id} not found" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {ProductId}", id);
            return StatusCode(500, new { message = "Internal server error occurred while retrieving the product" });
        }
    }

    /// <summary>
    /// Valida si un producto existe
    /// </summary>
    /// <param name="id">ID del producto</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de validación</returns>
    [HttpGet("{id:guid}/validate")]
    [ProducesResponseType(typeof(ValidationResponseDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ValidationResponseDto>> ValidateProduct(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Validating product {ProductId}", id);

            var query = new GetProductQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            var validationResponse = new ValidationResponseDto
            {
                IsValid = result != null,
                Message = result != null ? "Product exists" : "Product not found",
                Data = result
            };

            return Ok(validationResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating product {ProductId}", id);
            return StatusCode(500, new ValidationResponseDto 
            { 
                IsValid = false, 
                Message = "Internal server error occurred while validating the product" 
            });
        }
    }

    /// <summary>
    /// Valida stock disponible para múltiples productos
    /// </summary>
    /// <param name="request">Datos de validación de stock</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de validación de stock</returns>
    [HttpPost("validate-stock")]
    [ProducesResponseType(typeof(ValidationResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ValidationResponseDto>> ValidateStock(
        [FromBody] ValidateStockRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Validating stock for product {ProductId}, required quantity: {Quantity}",
                request.ProductId, request.RequiredQuantity);

            var query = new GetProductQuery(request.ProductId);
            var product = await _mediator.Send(query, cancellationToken);

            if (product == null)
            {
                return Ok(new ValidationResponseDto
                {
                    IsValid = false,
                    Message = "Product not found"
                });
            }

            var hasEnoughStock = product.Stock >= request.RequiredQuantity;
            var validationResponse = new ValidationResponseDto
            {
                IsValid = hasEnoughStock,
                Message = hasEnoughStock 
                    ? $"Stock available: {product.Stock}" 
                    : $"Insufficient stock. Available: {product.Stock}, Required: {request.RequiredQuantity}",
                Data = new { AvailableStock = product.Stock, RequiredQuantity = request.RequiredQuantity }
            };

            return Ok(validationResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating stock for product {ProductId}", request.ProductId);
            return StatusCode(500, new ValidationResponseDto 
            { 
                IsValid = false, 
                Message = "Internal server error occurred while validating stock" 
            });
        }
    }

    /// <summary>
    /// Obtiene múltiples productos por IDs (batch)
    /// </summary>
    /// <param name="request">Lista de IDs de productos</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de productos encontrados</returns>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsBatch(
        [FromBody] GetProductsBatchRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting batch of {Count} products", request.ProductIds.Count);

            var products = new List<ProductDto>();
            
            foreach (var productId in request.ProductIds)
            {
                var query = new GetProductQuery(productId);
                var product = await _mediator.Send(query, cancellationToken);
                
                if (product != null)
                {
                    products.Add(product);
                }
            }

            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products batch");
            return StatusCode(500, new { message = "Internal server error occurred while retrieving products" });
        }
    }

    /// <summary>
    /// Endpoint de salud para verificar el estado del servicio
    /// </summary>
    /// <returns>Estado del servicio</returns>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow, service = "ProductService" });
    }
}

/// <summary>
/// Request para validar stock
/// </summary>
public class ValidateStockRequest
{
    public Guid ProductId { get; set; }
    public int RequiredQuantity { get; set; }
}

/// <summary>
/// Request para obtener productos en lote
/// </summary>
public class GetProductsBatchRequest
{
    public List<Guid> ProductIds { get; set; } = new();
}