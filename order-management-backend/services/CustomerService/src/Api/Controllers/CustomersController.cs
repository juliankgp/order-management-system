using CustomerService.Application.Commands.LoginCustomer;
using CustomerService.Application.Commands.RegisterCustomer;
using CustomerService.Application.Commands.UpdateCustomerProfile;
using CustomerService.Application.DTOs;
using CustomerService.Application.Queries.GetCustomer;
using CustomerService.Application.Queries.GetCustomers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OrderManagement.Shared.Common.Models;
using System.Security.Claims;

namespace CustomerService.Api.Controllers;

/// <summary>
/// Controlador para el manejo de clientes
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(IMediator mediator, ILogger<CustomersController> logger)
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
        return Ok("CustomerService is running successfully!");
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
    /// Registra un nuevo cliente
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticatedCustomerDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<AuthenticatedCustomerDto>> Register(
        [FromBody] RegisterCustomerDto registerDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Customer registration attempt for email: {Email}", registerDto.Email);

            var command = new RegisterCustomerCommand(
                registerDto.Email,
                registerDto.Password,
                registerDto.FirstName,
                registerDto.LastName,
                registerDto.PhoneNumber,
                registerDto.DateOfBirth,
                registerDto.Gender);

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Customer registered successfully with ID: {CustomerId}", result.Id);
            return CreatedAtAction(nameof(GetCustomer), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Registration failed: {Message}", ex.Message);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during customer registration");
            return StatusCode(500, new { message = "An error occurred during registration." });
        }
    }

    /// <summary>
    /// Autentica un cliente
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticatedCustomerDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<AuthenticatedCustomerDto>> Login(
        [FromBody] LoginDto loginDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", loginDto.Email);

            var command = new LoginCustomerCommand(loginDto.Email, loginDto.Password);
            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Customer logged in successfully: {CustomerId}", result.Id);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login failed: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during customer login");
            return StatusCode(500, new { message = "An error occurred during login." });
        }
    }

    /// <summary>
    /// Obtiene todos los clientes con paginación y filtros
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<CustomerSummaryDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<PagedResult<CustomerSummaryDto>>> GetCustomers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting customers - Page: {Page}, PageSize: {PageSize}, SearchTerm: {SearchTerm}, IsActive: {IsActive}", 
                page, pageSize, searchTerm, isActive);

            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { message = "Invalid pagination parameters. Page and PageSize must be positive, and PageSize cannot exceed 100." });
            }

            var query = new GetCustomersQuery(page, pageSize, searchTerm, isActive);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers");
            return StatusCode(500, new { message = "An error occurred while retrieving customers." });
        }
    }

    /// <summary>
    /// Obtiene un cliente por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(CustomerDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<CustomerDto>> GetCustomer(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting customer with ID: {CustomerId}", id);

            var query = new GetCustomerQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result == null)
            {
                _logger.LogInformation("Customer with ID {CustomerId} not found", id);
                return NotFound(new { message = $"Customer with ID {id} not found." });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer with ID: {CustomerId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the customer." });
        }
    }

    /// <summary>
    /// Obtiene el perfil del cliente autenticado
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(CustomerDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<CustomerDto>> GetProfile(CancellationToken cancellationToken = default)
    {
        try
        {
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(customerIdClaim) || !Guid.TryParse(customerIdClaim, out var customerId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            _logger.LogInformation("Getting profile for customer: {CustomerId}", customerId);

            var query = new GetCustomerQuery(customerId);
            var result = await _mediator.Send(query, cancellationToken);

            if (result == null)
            {
                return NotFound(new { message = "Customer profile not found." });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer profile");
            return StatusCode(500, new { message = "An error occurred while retrieving the profile." });
        }
    }

    /// <summary>
    /// Actualiza el perfil del cliente autenticado
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(CustomerDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<CustomerDto>> UpdateProfile(
        [FromBody] UpdateCustomerProfileDto updateDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(customerIdClaim) || !Guid.TryParse(customerIdClaim, out var customerId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            _logger.LogInformation("Updating profile for customer: {CustomerId}", customerId);

            var command = new UpdateCustomerProfileCommand(
                customerId,
                updateDto.FirstName,
                updateDto.LastName,
                updateDto.PhoneNumber,
                updateDto.DateOfBirth,
                updateDto.Gender,
                updateDto.Preferences);

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Profile updated successfully for customer: {CustomerId}", customerId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Customer not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer profile");
            return StatusCode(500, new { message = "An error occurred while updating the profile." });
        }
    }
}