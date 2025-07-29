using AutoMapper;
using CustomerService.Application.DTOs;
using CustomerService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerService.Application.Commands.UpdateCustomerProfile;

/// <summary>
/// Handler para el comando de actualización de perfil de cliente
/// </summary>
public class UpdateCustomerProfileCommandHandler : IRequestHandler<UpdateCustomerProfileCommand, CustomerDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateCustomerProfileCommandHandler> _logger;

    public UpdateCustomerProfileCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateCustomerProfileCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CustomerDto> Handle(UpdateCustomerProfileCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating profile for customer {CustomerId}", request.CustomerId);

        // Buscar el cliente
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            _logger.LogWarning("Customer {CustomerId} not found", request.CustomerId);
            throw new KeyNotFoundException($"Customer with ID {request.CustomerId} not found.");
        }

        // Actualizar los campos
        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.PhoneNumber = request.PhoneNumber;
        customer.DateOfBirth = request.DateOfBirth;
        customer.Gender = request.Gender;
        customer.Preferences = request.Preferences;
        customer.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Customers.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Profile updated successfully for customer {CustomerId}", request.CustomerId);

        return _mapper.Map<CustomerDto>(customer);
    }
}