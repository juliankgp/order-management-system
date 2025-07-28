using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Shared.Common.Models;
using OrderService.Application.Commands.CreateOrder;
using OrderService.Application.Queries.GetOrder;

namespace OrderService.Web.Controllers;

/// <summary>
/// Controlador para el manejo de órdenes
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene una orden por ID
    /// </summary>
    /// <param name="id">ID de la orden</param>
    /// <returns>Datos de la orden</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrder(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting order with ID: {OrderId}", id);
            
            var query = new GetOrderQuery { OrderId = id };
            var result = await _mediator.Send(query);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order with ID: {OrderId}", id);
            return NotFound(ApiResponse.ErrorResponse($"Order with ID {id} not found"));
        }
    }

    /// <summary>
    /// Crea una nueva orden
    /// </summary>
    /// <param name="command">Datos de la orden a crear</param>
    /// <returns>Datos de la orden creada</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateOrderResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CreateOrderResponse>>> CreateOrder([FromBody] CreateOrderCommand command)
    {
        try
        {
            _logger.LogInformation("Creating new order for customer: {CustomerId}", command.CustomerId);
            
            var result = await _mediator.Send(command);
            
            if (result.Success && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetOrder), 
                    new { id = result.Data.OrderId }, 
                    result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for customer: {CustomerId}", command.CustomerId);
            return BadRequest(ApiResponse.ErrorResponse("Error creating order", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Obtiene órdenes con paginación
    /// </summary>
    /// <param name="pageNumber">Número de página</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <param name="sortBy">Campo para ordenar</param>
    /// <param name="sortDirection">Dirección del ordenamiento</param>
    /// <param name="searchTerm">Término de búsqueda</param>
    /// <returns>Lista paginada de órdenes</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<OrderDto>>>> GetOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string sortDirection = "asc",
        [FromQuery] string? searchTerm = null)
    {
        try
        {
            _logger.LogInformation("Getting orders - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
            
            var parameters = new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDirection = sortDirection,
                SearchTerm = searchTerm
            };

            // Aquí necesitarías implementar una query para obtener órdenes paginadas
            // var query = new GetOrdersQuery { Parameters = parameters };
            // var result = await _mediator.Send(query);
            
            // Por ahora retornamos una respuesta de ejemplo
            var emptyResult = new PagedResult<OrderDto>
            {
                Items = new List<OrderDto>(),
                TotalCount = 0,
                CurrentPage = pageNumber,
                PageSize = pageSize
            };
            
            return Ok(ApiResponse<PagedResult<OrderDto>>.SuccessResponse(emptyResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders");
            return BadRequest(ApiResponse.ErrorResponse("Error retrieving orders"));
        }
    }

    /// <summary>
    /// Obtiene órdenes por cliente
    /// </summary>
    /// <param name="customerId">ID del cliente</param>
    /// <param name="pageNumber">Número de página</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <returns>Lista de órdenes del cliente</returns>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<OrderDto>>>> GetOrdersByCustomer(
        Guid customerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting orders for customer: {CustomerId}", customerId);
            
            // Aquí implementarías la query para obtener órdenes por cliente
            var emptyResult = new PagedResult<OrderDto>
            {
                Items = new List<OrderDto>(),
                TotalCount = 0,
                CurrentPage = pageNumber,
                PageSize = pageSize
            };
            
            return Ok(ApiResponse<PagedResult<OrderDto>>.SuccessResponse(emptyResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for customer: {CustomerId}", customerId);
            return BadRequest(ApiResponse.ErrorResponse("Error retrieving customer orders"));
        }
    }
}
