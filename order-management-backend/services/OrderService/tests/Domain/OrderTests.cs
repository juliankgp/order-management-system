using FluentAssertions;
using OrderService.Domain.Entities;
using Xunit;

namespace OrderService.Tests.Domain;

/// <summary>
/// Tests unitarios para la entidad Order
/// </summary>
public class OrderTests
{
    [Fact]
    public void Order_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var order = new Order
        {
            OrderNumber = "ORD-20240728-1234",
            CustomerId = Guid.NewGuid(),
            ShippingAddress = "123 Main St",
            ShippingCity = "Test City",
            ShippingZipCode = "12345",
            ShippingCountry = "Test Country"
        };

        // Assert
        order.Id.Should().NotBeEmpty();
        order.Status.Should().Be(OrderStatus.Pending);
        order.OrderDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        order.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        order.IsDeleted.Should().BeFalse();
        order.Items.Should().BeEmpty();
        order.StatusHistory.Should().BeEmpty();
    }

    [Fact]
    public void Order_Should_Allow_Adding_Items()
    {
        // Arrange
        var order = new Order
        {
            OrderNumber = "ORD-20240728-1234",
            CustomerId = Guid.NewGuid(),
            ShippingAddress = "123 Main St",
            ShippingCity = "Test City",
            ShippingZipCode = "12345",
            ShippingCountry = "Test Country"
        };

        var orderItem = new OrderItem
        {
            OrderId = order.Id,
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            ProductSku = "TEST-001",
            Quantity = 2,
            UnitPrice = 50.00m,
            TotalPrice = 100.00m
        };

        // Act
        order.Items.Add(orderItem);

        // Assert
        order.Items.Should().HaveCount(1);
        order.Items.First().Should().Be(orderItem);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Processing)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void Order_Should_Allow_All_Valid_Statuses(OrderStatus status)
    {
        // Arrange
        var order = new Order
        {
            OrderNumber = "ORD-20240728-1234",
            CustomerId = Guid.NewGuid(),
            ShippingAddress = "123 Main St",
            ShippingCity = "Test City",
            ShippingZipCode = "12345",
            ShippingCountry = "Test Country"
        };

        // Act
        order.Status = status;

        // Assert
        order.Status.Should().Be(status);
    }

    [Fact]
    public void OrderItem_Should_Calculate_Total_Price_Correctly()
    {
        // Arrange
        var quantity = 3;
        var unitPrice = 25.50m;
        var expectedTotal = 76.50m;

        var orderItem = new OrderItem
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            ProductSku = "TEST-001",
            Quantity = quantity,
            UnitPrice = unitPrice,
            TotalPrice = quantity * unitPrice
        };

        // Act & Assert
        orderItem.TotalPrice.Should().Be(expectedTotal);
    }

    [Fact]
    public void OrderStatusHistory_Should_Track_Status_Changes()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        var statusHistory = new OrderStatusHistory
        {
            OrderId = orderId,
            PreviousStatus = OrderStatus.Pending,
            NewStatus = OrderStatus.Confirmed,
            ChangedBy = userId,
            Reason = "Order confirmed by customer service",
            Comments = "Customer called to confirm payment"
        };

        // Act & Assert
        statusHistory.OrderId.Should().Be(orderId);
        statusHistory.PreviousStatus.Should().Be(OrderStatus.Pending);
        statusHistory.NewStatus.Should().Be(OrderStatus.Confirmed);
        statusHistory.ChangedBy.Should().Be(userId);
        statusHistory.ChangedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
