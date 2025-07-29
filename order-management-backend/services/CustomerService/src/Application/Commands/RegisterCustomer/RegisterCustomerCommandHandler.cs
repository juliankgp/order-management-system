using AutoMapper;
using CustomerService.Application.DTOs;
using CustomerService.Application.Interfaces;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagement.Shared.Events.Customers;
using OrderManagement.Shared.Security.Services;

namespace CustomerService.Application.Commands.RegisterCustomer;

/// <summary>
/// Handler para el comando de registro de cliente
/// </summary>
public class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand, AuthenticatedCustomerDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    private readonly ILogger<RegisterCustomerCommandHandler> _logger;
    private readonly IEventBusService _eventBus;

    public RegisterCustomerCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHashingService passwordHashingService,
        IJwtService jwtService,
        IMapper mapper,
        ILogger<RegisterCustomerCommandHandler> logger,
        IEventBusService eventBus)
    {
        _unitOfWork = unitOfWork;
        _passwordHashingService = passwordHashingService;
        _jwtService = jwtService;
        _mapper = mapper;
        _logger = logger;
        _eventBus = eventBus;
    }

    public async Task<AuthenticatedCustomerDto> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registering new customer with email: {Email}", request.Email);

        // Verificar si el email ya existe
        var existingCustomer = await _unitOfWork.Customers.GetByEmailAsync(request.Email, cancellationToken);
        if (existingCustomer != null)
        {
            _logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
            throw new InvalidOperationException($"A customer with email {request.Email} already exists.");
        }

        // Crear el cliente
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = _passwordHashingService.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            IsActive = true,
            EmailVerified = false,
            EmailVerificationToken = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer registered successfully with ID: {CustomerId}", customer.Id);

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

        // Publicar evento de cliente registrado
        var customerRegisteredEvent = new CustomerRegisteredEvent
        {
            CustomerId = customer.Id,
            Email = customer.Email,
            FullName = customer.FullName,
            RegisteredAt = customer.CreatedAt
        };

        try
        {
            await _eventBus.PublishAsync(customerRegisteredEvent, "customers.registered", cancellationToken);
            _logger.LogInformation("CustomerRegisteredEvent published for customer {CustomerId}", customer.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish CustomerRegisteredEvent for customer {CustomerId}", customer.Id);
            // No fallar el registro por problemas de eventos
        }

        return authenticatedCustomer;
    }
}