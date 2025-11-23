using AuthService.Application.Commands.Tenant.CreateUserCommands;
using AuthService.Application.Commands.Users.CreateUserCommands;
using AuthService.Domain.Enums;
using AuthService.Infrastructure.Database.Entities;
using AuthService.Infrastructure.Database.Repositories;
using AuthService.Infrastructure.Database.Transactions;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Shared;
using User = AuthService.Domain.Entities.User;
using UserModel = AuthService.Infrastructure.Database.Entities.User;
using TenantEntity = AuthService.Infrastructure.Database.Entities.Tenant;
using Tenant = AuthService.Domain.Entities.Tenant;

namespace AuthService.UnitTest.Commands.Users;

public sealed class RegisterUserCommandTest
{
    private readonly Mock<UserManager<UserModel>> _repository;
    private readonly Mock<IRepository<TenantEntity, Tenant>> _tenantRepository;
    private readonly Mock<RoleManager<Role>> _roleManager;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger> _logger;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<IPasswordHasher<UserModel>> _passwordHasher;
    private readonly RegisterUserCommandHandler _handler;


    public RegisterUserCommandTest()
    {
        _repository = new Mock<UserManager<UserModel>>();
        _tenantRepository = new Mock<IRepository<TenantEntity, Tenant>>();
        _roleManager = new Mock<RoleManager<Role>>();
        _logger = new Mock<ILogger>();
        _unitOfWork = new Mock<IUnitOfWork>();

        _handler = new RegisterUserCommandHandler(
            _repository.Object,
            _mapper.Object,
            _passwordHasher.Object,
            _unitOfWork.Object);
    }
    
    [Fact]
    public async Task HandleShouldReturnSuccessWhenUserIsCreatedSuccessfully()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "test@example.com",
            "Test User",
            "SecurePassword123!",
            "testuser",
            Guid.NewGuid(),
            UserRole.User);

        var tenantId = command.tenantId;
        var userId = Guid.NewGuid();

        var domainUser = new User
        {
            Id = userId,
            Email = command.email,
            UserName = command.username,
            FullName = command.fullName,
            TenantId = tenantId,
            Role = command.role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var userModel = new UserModel
        {
            Id = userId,
            Email = command.email,
            UserName = command.username,
            TenantId = tenantId,
            IsActive = true
        };

        _repository
            .Setup(x => x.CreateAsync(It.IsAny<UserModel>(), default))
            .Returns(new Task<IdentityResult>(() => IdentityResult.Success));

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _repository.Verify(
            x => x.CreateAsync(
                It.Is<UserModel>(u =>
                    u.Email == command.email &&
                    u.UserName == command.username &&
                    u.TenantId == tenantId),
                default),
            Times.Once);
    }

    [Fact]
    public async Task HandleShouldAssignCorrectTenantIdWhenCreatingUser()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var command = new RegisterUserCommand(
            "test@example.com",
            "Test User",
            "SecurePassword123!",
            "testuser",
            tenantId,
            UserRole.User);

        var domainUser = new User
        {
            Id = Guid.NewGuid(),
            Email = command.email,
            UserName = command.username,
            FullName = command.fullName,
            TenantId = tenantId,
            Role = command.role,
            IsActive = true
        };

        var userModel = new UserModel
        {
            Id = domainUser.Id,
            Email = command.email,
            UserName = command.username,
            TenantId = tenantId,
            IsActive = true
        };

        Domain.Entities.User? capturedUser = null;
        _repository
            .Setup(x => x.CreateAsync(It.IsAny<UserModel>(), default))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user)
            .Returns(new Task<IdentityResult>(() => IdentityResult.Success));

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task HandleShouldSetUserAsActiveWhenCreatingUser()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var command = new RegisterUserCommand(
            "test@example.com",
            "Test User",
            "SecurePassword123!",
            "testuser",
            tenantId,
            UserRole.Admin);

        var domainUser = new User
        {
            Id = Guid.NewGuid(),
            Email = command.email,
            UserName = command.username,
            FullName = command.fullName,
            TenantId = tenantId,
            Role = command.role,
            IsActive = true
        };

        var userModel = new UserModel
        {
            Id = domainUser.Id,
            Email = command.email,
            UserName = command.username,
            TenantId = tenantId,
            IsActive = true
        };

        User? capturedUser = null;
        _repository
            .Setup(x => x.CreateAsync(It.IsAny<UserModel>(), default))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user)
            .Returns(new Task<IdentityResult>(() => IdentityResult.Success));

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task HandleShouldReturnFailureWhenUserManagerThrowsException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new RegisterUserCommand(
            "test@example.com",
            "Test User",
            "SecurePassword123!",
            "testuser",
            tenantId,
            UserRole.Admin);

        var domainUser = new User
        {
            Id = Guid.NewGuid(),
            Email = command.email,
            UserName = command.username,
            FullName = command.fullName,
            TenantId = tenantId,
            Role = command.role
        };
        
        _repository
            .Setup(x => x.CreateAsync(It.IsAny<UserModel>(), default))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database connection failed");
    }
}