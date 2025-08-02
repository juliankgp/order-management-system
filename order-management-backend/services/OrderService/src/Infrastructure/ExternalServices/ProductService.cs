using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderService.Application.DTOs;
using System.Text;

namespace OrderService.Infrastructure.ExternalServices;

/// <summary>
/// Implementación del servicio para comunicación con Product Service
/// </summary>
public class ProductService : OrderService.Application.Interfaces.IProductService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductService> _logger;
    private readonly string _baseUrl;

    public ProductService(HttpClient httpClient, ILogger<ProductService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = Environment.GetEnvironmentVariable("PRODUCT_SERVICE_URL") ?? "https://localhost:5002";
    }

    public async Task<ValidationResponseDto> ValidateProductExistsAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating product existence for {ProductId}", productId);

            var response = await _httpClient.GetAsync($"{_baseUrl}/api/products/{productId}/validate", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var validationResponse = JsonConvert.DeserializeObject<ValidationResponseDto>(content);
                return validationResponse ?? new ValidationResponseDto { IsValid = false, Message = "Invalid response from Product Service" };
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new ValidationResponseDto { IsValid = false, Message = "Product not found" };
            }

            _logger.LogWarning("Product service returned error: {StatusCode}", response.StatusCode);
            return new ValidationResponseDto { IsValid = false, Message = "Product service error" };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error when validating product {ProductId}", productId);
            return new ValidationResponseDto { IsValid = false, Message = "Product service unavailable" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating product {ProductId}", productId);
            return new ValidationResponseDto { IsValid = false, Message = "Validation error" };
        }
    }

    public async Task<ProductResponseDto?> GetProductAsync(Guid productId)
    {
        try
        {
            _logger.LogInformation("Getting product information for {ProductId}", productId);

            var response = await _httpClient.GetAsync($"{_baseUrl}/api/products/{productId}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var product = JsonConvert.DeserializeObject<ProductResponseDto>(content);
                return product;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Product {ProductId} not found", productId);
                return null;
            }

            _logger.LogWarning("Product service returned error: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {ProductId}", productId);
            return null;
        }
    }

    public async Task<ValidationResponseDto> ValidateStockAsync(Guid productId, int requiredQuantity, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating stock for product {ProductId}, quantity {Quantity}", productId, requiredQuantity);

            var requestBody = new { ProductId = productId, RequiredQuantity = requiredQuantity };
            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/products/validate-stock", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var validationResponse = JsonConvert.DeserializeObject<ValidationResponseDto>(responseContent);
                return validationResponse ?? new ValidationResponseDto { IsValid = false, Message = "Invalid response from Product Service" };
            }

            _logger.LogWarning("Product service returned error when validating stock: {StatusCode}", response.StatusCode);
            return new ValidationResponseDto { IsValid = false, Message = "Stock validation service error" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating stock for product {ProductId}", productId);
            return new ValidationResponseDto { IsValid = false, Message = "Stock validation error" };
        }
    }

    public async Task<IEnumerable<ProductResponseDto>> GetProductsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting product information for {Count} products", productIds.Count());

            var requestBody = new { ProductIds = productIds.ToArray() };
            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/products/batch", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var products = JsonConvert.DeserializeObject<List<ProductResponseDto>>(responseContent);
                return products ?? new List<ProductResponseDto>();
            }

            _logger.LogWarning("Product service returned error when getting products: {StatusCode}", response.StatusCode);
            return new List<ProductResponseDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products batch");
            return new List<ProductResponseDto>();
        }
    }
}
