using AutoMapper;
using CustomerService.Application.DTOs;
using CustomerService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerService.Application.Queries.GetCustomer;

/// <summary>
/// Handler para obtener un cliente por ID
/// </summary>
public class GetCustomerQueryHandler : IRequestHandler<GetCustomerQuery, CustomerDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCustomerQueryHandler> _logger;

    public GetCustomerQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetCustomerQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CustomerDto?> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting customer with ID: {CustomerId}", request.CustomerId);

        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId, cancellationToken);
        
        if (customer == null)
        {
            _logger.LogInformation("Customer with ID {CustomerId} not found", request.CustomerId);
            return null;
        }

        return _mapper.Map<CustomerDto>(customer);
    }
}