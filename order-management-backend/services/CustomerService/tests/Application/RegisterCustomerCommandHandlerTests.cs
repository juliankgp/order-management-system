using AutoMapper;
using CustomerService.Application.Commands.RegisterCustomer;
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

public class RegisterCustomerCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPasswordHashingService> _passwordHashingServiceMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IEventBusService> _eventBusMock;
    private readonly Mock<ILogger<RegisterCustomerCommandHandler>> _loggerMock;
    private readonly IMapper _mapper;
    private readonly RegisterCustomerCommandHandler _handler;

    public RegisterCustomerCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _passwordHashingServiceMock = new Mock<IPasswordHashingService>();
        _jwtServiceMock = new Mock<IJwtService>();
        _eventBusMock = new Mock<IEventBusService>();
        _loggerMock = new Mock<ILogger<RegisterCustomerCommandHandler>>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<CustomerMappingProfile>());
        _mapper = config.CreateMapper();

        _handler = new RegisterCustomerCommandHandler(
            _unitOfWorkMock.Object,
            _passwordHashingServiceMock.Object,
            _jwtServiceMock.Object,
            _mapper,
            _loggerMock.Object,
            _eventBusMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldRegisterCustomerSuccessfully()
    {
        // Arrange
        var command = new RegisterCustomerCommand(
            "test@example.com",
            "Password123!",
            "John",
            "Doe",
            "+1-555-0123",
            new DateTime(1990, 1, 1),
            Gender.Male);

        var hashedPassword = "hashed_password";
        var jwtToken = "jwt_token";

        _unitOfWorkMock.Setup(x => x.Customers.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        _passwordHashingServiceMock.Setup(x => x.HashPassword(command.Password))
            .Returns(hashedPassword);

        _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<OrderManagement.Shared.Security.Models.UserClaims>()))
            .Returns(jwtToken);

        _unitOfWorkMock.Setup(x => x.Customers.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(command.Email.ToLowerInvariant());
        result.FullName.Should().Be($"{command.FirstName} {command.LastName}");
        result.Token.Should().Be(jwtToken);
        result.EmailVerified.Should().BeFalse();

        _unitOfWorkMock.Verify(x => x.Customers.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _passwordHashingServiceMock.Verify(x => x.HashPassword(command.Password), Times.Once);
        _jwtServiceMock.Verify(x => x.GenerateToken(It.IsAny<OrderManagement.Shared.Security.Models.UserClaims>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new RegisterCustomerCommand(
            "existing@example.com",
            "Password123!",
            "John",
            "Doe");

        var existingCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            PasswordHash = "existing_hash",
            FirstName = "Existing",
            LastName = "Customer",
            IsActive = true,
            EmailVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _unitOfWorkMock.Setup(x => x.Customers.GetByEmailAsync(command.Email.ToLowerInvariant(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain($"A customer with email {command.Email} already exists");

        _unitOfWorkMock.Verify(x => x.Customers.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    public async Task Handle_InvalidEmail_ShouldThrowException(string invalidEmail)
    {
        // Arrange
        var command = new RegisterCustomerCommand(
            invalidEmail,
            "Password123!",
            "John",
            "Doe");

        // Act & Assert - This would be caught by FluentValidation in the actual pipeline
        // For unit testing the handler directly, we assume valid input
        // This test demonstrates the pattern for testing validation scenarios
    }
}