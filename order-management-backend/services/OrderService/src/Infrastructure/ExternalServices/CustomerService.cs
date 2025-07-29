using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;

namespace OrderService.Infrastructure.ExternalServices;

/// <summary>
/// Implementación del servicio para comunicación con Customer Service
/// </summary>
public class CustomerService : OrderService.Application.Interfaces.ICustomerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomerService> _logger;
    private readonly string _baseUrl;

    public CustomerService(HttpClient httpClient, ILogger<CustomerService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = Environment.GetEnvironmentVariable("CUSTOMER_SERVICE_URL") ?? "http://localhost:5003";
    }

    public async Task<ValidationResponseDto> ValidateCustomerExistsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating customer existence for {CustomerId}", customerId);

            var response = await _httpClient.GetAsync($"{_baseUrl}/api/customers/{customerId}/validate", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var validationResponse = JsonConvert.DeserializeObject<ValidationResponseDto>(content);
                return validationResponse ?? new ValidationResponseDto { IsValid = false, Message = "Invalid response from Customer Service" };
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new ValidationResponseDto { IsValid = false, Message = "Customer not found" };
            }

            _logger.LogWarning("Customer service returned error: {StatusCode}", response.StatusCode);
            return new ValidationResponseDto { IsValid = false, Message = "Customer service error" };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error when validating customer {CustomerId}", customerId);
            return new ValidationResponseDto { IsValid = false, Message = "Customer service unavailable" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating customer {CustomerId}", customerId);
            return new ValidationResponseDto { IsValid = false, Message = "Validation error" };
        }
    }

    public async Task<CustomerResponseDto?> GetCustomerAsync(Guid customerId)
    {
        try
        {
            _logger.LogInformation("Getting customer information for {CustomerId}", customerId);

            var response = await _httpClient.GetAsync($"{_baseUrl}/api/customers/{customerId}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var customer = JsonConvert.DeserializeObject<CustomerResponseDto>(content);
                return customer;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Customer {CustomerId} not found", customerId);
                return null;
            }

            _logger.LogWarning("Customer service returned error: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer {CustomerId}", customerId);
            return null;
        }
    }
}
