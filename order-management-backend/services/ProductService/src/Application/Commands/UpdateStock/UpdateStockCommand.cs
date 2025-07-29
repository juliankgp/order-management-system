using MediatR;

namespace ProductService.Application.Commands.UpdateStock;

/// <summary>
/// Comando para actualizar stock de producto
/// </summary>
public record UpdateStockCommand(
    Guid ProductId,
    int Quantity,
    string Reason,
    string? ExternalReference = null,
    Guid? UserId = null) : IRequest<bool>;