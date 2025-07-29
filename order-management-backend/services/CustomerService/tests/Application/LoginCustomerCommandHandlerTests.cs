using AutoMapper;
using CustomerService.Application.Commands.LoginCustomer;
using CustomerService.Application.Interfaces;
using CustomerService.Application.Mappings;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.Shared.Security.Services;
using Xunit;

namespace CustomerService.Tests.Application;

public class LoginCustomerCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPasswordHashingService> _passwordHashingServiceMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<ILogger<LoginCustomerCommandHandler>> _loggerMock;
    private readonly IMapper _mapper;
    private readonly LoginCustomerCommandHandler _handler;

    public LoginCustomerCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _passwordHashingServiceMock = new Mock<IPasswordHashingService>();
        _jwtServiceMock = new Mock<IJwtService>();
        _loggerMock = new Mock<ILogger<LoginCustomerCommandHandler>>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<CustomerMappingProfile>());
        _mapper = config.CreateMapper();

        _handler = new LoginCustomerCommandHandler(
            _unitOfWorkMock.Object,
            _passwordHashingServiceMock.Object,
            _jwtServiceMock.Object,
            _mapper,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnAuthenticatedCustomer()
    {
        // Arrange
        var command = new LoginCustomerCommand("test@example.com", "Password123!");
        var jwtToken = "jwt_token";

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = command.Email.ToLowerInvariant(),
            PasswordHash = "hashed_password",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true,
            EmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _unitOfWorkMock.Setup(x => x.Customers.GetByEmailAsync(command.Email.ToLowerInvariant(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _passwordHashingServiceMock.Setup(x => x.VerifyPassword(command.Password, customer.PasswordHash))
            .Returns(true);

        _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<OrderManagement.Shared.Security.Models.UserClaims>()))
            .Returns(jwtToken);

        _unitOfWorkMock.Setup(x => x.Customers.UpdateLastLoginAsync(customer.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customer.Id);
        result.Email.Should().Be(customer.Email);
        result.FullName.Should().Be(customer.FullName);
        result.Token.Should().Be(jwtToken);
        result.EmailVerified.Should().Be(customer.EmailVerified);

        _unitOfWorkMock.Verify(x => x.Customers.UpdateLastLoginAsync(customer.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _passwordHashingServiceMock.Verify(x => x.VerifyPassword(command.Password, customer.PasswordHash), Times.Once);
        _jwtServiceMock.Verify(x => x.GenerateToken(It.IsAny<OrderManagement.Shared.Security.Models.UserClaims>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CustomerNotFound_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new LoginCustomerCommand("nonexistent@example.com", "Password123!");

        _unitOfWorkMock.Setup(x => x.Customers.GetByEmailAsync(command.Email.ToLowerInvariant(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("Invalid email or password.");

        _passwordHashingServiceMock.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _jwtServiceMock.Verify(x => x.GenerateToken(It.IsAny<OrderManagement.Shared.Security.Models.UserClaims>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InactiveCustomer_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new LoginCustomerCommand("test@example.com", "Password123!");

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = command.Email.ToLowerInvariant(),
            PasswordHash = "hashed_password",
            FirstName = "John",
            LastName = "Doe",
            IsActive = false, // Inactive customer
            EmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _unitOfWorkMock.Setup(x => x.Customers.GetByEmailAsync(command.Email.ToLowerInvariant(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("Account is inactive. Please contact support.");

        _passwordHashingServiceMock.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _jwtServiceMock.Verify(x => x.GenerateToken(It.IsAny<OrderManagement.Shared.Security.Models.UserClaims>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new LoginCustomerCommand("test@example.com", "WrongPassword");

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = command.Email.ToLowerInvariant(),
            PasswordHash = "hashed_password",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true,
            EmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _unitOfWorkMock.Setup(x => x.Customers.GetByEmailAsync(command.Email.ToLowerInvariant(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _passwordHashingServiceMock.Setup(x => x.VerifyPassword(command.Password, customer.PasswordHash))
            .Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("Invalid email or password.");

        _passwordHashingServiceMock.Verify(x => x.VerifyPassword(command.Password, customer.PasswordHash), Times.Once);
        _jwtServiceMock.Verify(x => x.GenerateToken(It.IsAny<OrderManagement.Shared.Security.Models.UserClaims>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.Customers.UpdateLastLoginAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}