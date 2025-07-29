using AutoMapper;
using CustomerService.Application.DTOs;
using CustomerService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagement.Shared.Common.Models;

namespace CustomerService.Application.Queries.GetCustomers;

/// <summary>
/// Handler para obtener clientes con paginación
/// </summary>
public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PagedResult<CustomerSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCustomersQueryHandler> _logger;

    public GetCustomersQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetCustomersQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<CustomerSummaryDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting customers - Page: {Page}, PageSize: {PageSize}, SearchTerm: {SearchTerm}, IsActive: {IsActive}", 
            request.Page, request.PageSize, request.SearchTerm, request.IsActive);

        var customers = await _unitOfWork.Customers.GetPagedAsync(
            request.Page, 
            request.PageSize, 
            request.SearchTerm, 
            request.IsActive, 
            cancellationToken);

        var customerDtos = customers.Items.Select(_mapper.Map<CustomerSummaryDto>).ToList();

        return new PagedResult<CustomerSummaryDto>
        {
            Items = customerDtos,
            TotalCount = customers.TotalCount,
            CurrentPage = customers.CurrentPage,
            PageSize = customers.PageSize
        };
    }
}