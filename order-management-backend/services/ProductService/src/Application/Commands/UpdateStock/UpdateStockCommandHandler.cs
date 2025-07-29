using MediatR;
using Microsoft.Extensions.Logging;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Commands.UpdateStock;

/// <summary>
/// Handler para actualizar stock de producto
/// </summary>
public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateStockCommandHandler> _logger;

    public UpdateStockCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateStockCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating stock for product {ProductId}, quantity: {Quantity}, reason: {Reason}",
            request.ProductId, request.Quantity, request.Reason);

        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Product {ProductId} not found", request.ProductId);
            return false;
        }

        var previousStock = product.Stock;
        var newStock = previousStock + request.Quantity;

        if (newStock < 0)
        {
            _logger.LogWarning("Cannot update stock for product {ProductId}. New stock would be negative: {NewStock}",
                request.ProductId, newStock);
            return false;
        }

        // Actualizar stock del producto
        product.Stock = newStock;
        product.UpdatedAt = DateTime.UtcNow;

        // Crear movimiento de stock
        var stockMovement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            MovementType = request.Quantity > 0 ? StockMovementType.In : StockMovementType.Out,
            Quantity = request.Quantity,
            PreviousStock = previousStock,
            NewStock = newStock,
            Reason = request.Reason,
            ExternalReference = request.ExternalReference,
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.StockMovements.AddAsync(stockMovement, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Stock updated successfully for product {ProductId}. Previous: {PreviousStock}, New: {NewStock}",
            request.ProductId, previousStock, newStock);

        return true;
    }
}