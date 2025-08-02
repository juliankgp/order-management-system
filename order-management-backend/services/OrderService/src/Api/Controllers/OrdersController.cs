using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Shared.Common.Models;
using OrderService.Application.Commands.CreateOrder;
using OrderService.Application.Commands.DeleteOrder;
using OrderService.Application.Commands.UpdateOrder;
using OrderService.Application.DTOs;
using OrderService.Application.Queries.GetOrder;
using OrderService.Application.Queries.GetOrders;

namespace OrderService.Api.Controllers;

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
    /// Test endpoint to verify service is running
    /// </summary>
    [HttpGet("test")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    public ActionResult<string> Test()
    {
        return Ok("OrderService is running successfully!");
    }

    /// <summary>
    /// JWT configuration debug endpoint
    /// </summary>
    [HttpGet("jwt-debug")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    public ActionResult<object> JwtDebug([FromServices] IConfiguration configuration)
    {
        var key = configuration["Jwt:Key"];
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        
        // Generar hash de la clave para verificar consistencia
        var keyHash = !string.IsNullOrEmpty(key) ? 
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(key)))[..16] : 
            "NO_KEY";
        
        return Ok(new { 
            KeyLength = key?.Length ?? 0,
            KeyExists = !string.IsNullOrEmpty(key),
            KeyHash = keyHash,
            Issuer = issuer,
            Audience = audience,
            HasFallback = true
        });
    }

    /// <summary>
    /// Crea una nueva orden
    /// </summary>
    /// <param name="createOrderDto">Datos de la orden a crear</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>La orden creada</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<OrderDto>> CreateOrder(
        [FromBody] CreateOrderDto createOrderDto,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating order for customer {CustomerId}", createOrderDto.CustomerId);

            var command = new CreateOrderCommand(createOrderDto);
            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Order {OrderId} created successfully", result.Id);
            return CreatedAtAction(nameof(GetOrder), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for customer {CustomerId}", createOrderDto.CustomerId);
            return StatusCode(500, new { message = "Internal server error occurred while creating the order" });
        }
    }

    /// <summary>
    /// Obtiene una orden por ID
    /// </summary>
    /// <param name="id">ID de la orden</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>La orden solicitada</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<OrderDto>> GetOrder(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting order {OrderId}", id);

            var query = new GetOrderQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result == null)
            {
                _logger.LogWarning("Order {OrderId} not found", id);
                return NotFound(new { message = $"Order {id} not found" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order {OrderId}", id);
            return StatusCode(500, new { message = "Internal server error occurred while retrieving the order" });
        }
    }

    /// <summary>
    /// Obtiene órdenes con paginación y filtros
    /// </summary>
    /// <param name="page">Número de página</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <param name="customerId">ID del cliente (filtro opcional)</param>
    /// <param name="status">Estado de la orden (filtro opcional)</param>
    /// <param name="fromDate">Fecha desde (filtro opcional)</param>
    /// <param name="toDate">Fecha hasta (filtro opcional)</param>
    /// <param name="orderNumber">Número de orden (filtro opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista paginada de órdenes</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? customerId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? orderNumber = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validaciones básicas
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            _logger.LogInformation("Getting orders with filters - Page: {Page}, PageSize: {PageSize}, CustomerId: {CustomerId}, Status: {Status}",
                page, pageSize, customerId, status);

            var query = new GetOrdersQuery
            {
                Page = page,
                PageSize = pageSize,
                CustomerId = customerId,
                Status = status,
                FromDate = fromDate,
                ToDate = toDate,
                OrderNumber = orderNumber
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders");
            return StatusCode(500, new { message = "Internal server error occurred while retrieving orders" });
        }
    }

    /// <summary>
    /// Actualiza una orden existente
    /// </summary>
    /// <param name="id">ID de la orden a actualizar</param>
    /// <param name="updateOrderDto">Datos de actualización</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>La orden actualizada</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<OrderDto>> UpdateOrder(
        [FromRoute] Guid id,
        [FromBody] UpdateOrderDto updateOrderDto,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating order {OrderId}", id);

            var command = new UpdateOrderCommand(id, updateOrderDto);
            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Order {OrderId} updated successfully", id);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning("Order {OrderId} not found for update", id);
            return NotFound(new { message = $"Order {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId}", id);
            return StatusCode(500, new { message = "Internal server error occurred while updating the order" });
        }
    }

    /// <summary>
    /// Elimina una orden (soft delete)
    /// </summary>
    /// <param name="id">ID de la orden a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la operación</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteOrder(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting order {OrderId}", id);

            var command = new DeleteOrderCommand(id);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
            {
                _logger.LogWarning("Order {OrderId} not found for deletion", id);
                return NotFound(new { message = $"Order {id} not found" });
            }

            _logger.LogInformation("Order {OrderId} deleted successfully", id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Cannot delete order {OrderId}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order {OrderId}", id);
            return StatusCode(500, new { message = "Internal server error occurred while deleting the order" });
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
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
