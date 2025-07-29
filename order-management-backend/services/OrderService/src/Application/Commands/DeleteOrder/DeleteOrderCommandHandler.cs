using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Commands.DeleteOrder;

/// <summary>
/// Handler para el comando de eliminar orden
/// </summary>
public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBusService _eventBus;
    private readonly ILogger<DeleteOrderCommandHandler> _logger;

    public DeleteOrderCommandHandler(
        IUnitOfWork unitOfWork,
        IEventBusService eventBus,
        ILogger<DeleteOrderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing delete order command for order {OrderId}", request.OrderId);

        try
        {
            // 1. Obtener la orden
            var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for deletion", request.OrderId);
                return false;
            }

            // 2. Verificar si la orden puede ser eliminada
            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Shipped)
            {
                _logger.LogWarning("Cannot delete order {OrderId} with status {Status}", request.OrderId, order.Status);
                throw new InvalidOperationException($"Cannot delete order with status {order.Status}");
            }

            // 3. Realizar soft delete
            order.IsDeleted = true;
            order.DeletedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order {OrderId} deleted successfully", order.Id);

            // 4. Publicar evento de orden eliminada (si tienes este evento definido)
            // var orderDeletedEvent = new OrderDeletedEvent
            // {
            //     OrderId = order.Id,
            //     CustomerId = order.CustomerId,
            //     DeletedAt = order.DeletedAt.Value
            // };
            // await _eventBus.PublishAsync(orderDeletedEvent, cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order {OrderId}", request.OrderId);
            throw;
        }
    }
}
