using AutoMapper;
using CustomerService.Application.DTOs;
using CustomerService.Application.Interfaces;
using CustomerService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagement.Shared.Security.Services;

namespace CustomerService.Application.Commands.LoginCustomer;

/// <summary>
/// Handler para el comando de autenticación de cliente
/// </summary>
public class LoginCustomerCommandHandler : IRequestHandler<LoginCustomerCommand, AuthenticatedCustomerDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    private readonly ILogger<LoginCustomerCommandHandler> _logger;

    public LoginCustomerCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHashingService passwordHashingService,
        IJwtService jwtService,
        IMapper mapper,
        ILogger<LoginCustomerCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHashingService = passwordHashingService;
        _jwtService = jwtService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AuthenticatedCustomerDto> Handle(LoginCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);

        // Buscar el cliente por email
        var customer = await _unitOfWork.Customers.GetByEmailAsync(request.Email.ToLowerInvariant(), cancellationToken);
        if (customer == null)
        {
            _logger.LogWarning("Login failed: Customer with email {Email} not found", request.Email);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Verificar si el cliente está activo
        if (!customer.IsActive)
        {
            _logger.LogWarning("Login failed: Customer {CustomerId} is inactive", customer.Id);
            throw new UnauthorizedAccessException("Account is inactive. Please contact support.");
        }

        // Verificar la contraseña
        if (!_passwordHashingService.VerifyPassword(request.Password, customer.PasswordHash))
        {
            _logger.LogWarning("Login failed: Invalid password for customer {CustomerId}", customer.Id);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Actualizar la fecha de último login
        await _unitOfWork.Customers.UpdateLastLoginAsync(customer.Id, DateTime.UtcNow, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer {CustomerId} logged in successfully", customer.Id);

        // Generar JWT token
        var userClaims = new OrderManagement.Shared.Security.Models.UserClaims
        {
            UserId = customer.Id,
            Email = customer.Email,
            FullName = customer.FullName
        };
        var token = _jwtService.GenerateToken(userClaims);
        var tokenExpires = DateTime.UtcNow.AddHours(24); // 24 horas por defecto

        // Crear el DTO de respuesta
        var authenticatedCustomer = _mapper.Map<AuthenticatedCustomerDto>(customer);
        authenticatedCustomer.Token = token;
        authenticatedCustomer.TokenExpires = tokenExpires;

        return authenticatedCustomer;
    }
}