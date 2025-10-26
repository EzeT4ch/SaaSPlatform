using AuthService.Application.Commands.Tenant.CreateUserCommands;
using AuthService.Application.Commands.Users.CreateUserCommands;
using AuthService.Domain.Enums;
using AuthService.Infrastructure.Database.Repositories;
using AuthService.Infrastructure.Database.Transactions;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shared;
using User = AuthService.Domain.Entities.User;
using UserModel = AuthService.Infrastructure.Database.Entities.User;

namespace AuthService.UnitTest.Commands.Users;

public sealed class RegisterUserCommandTest
{
    private readonly Mock<IRepository<UserModel, User>> _repository;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<IPasswordHasher<UserModel>> _passwordHasher;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly RegisterUserCommandHandler _handler;


    public RegisterUserCommandTest()
    {
        _repository = new Mock<IRepository<UserModel, User>>();
        _mapper = new Mock<IMapper>();
        _passwordHasher = new Mock<IPasswordHasher<UserModel>>();
        _unitOfWork = new Mock<IUnitOfWork>();

        _handler = new RegisterUserCommandHandler(
            _repository.Object,
            _mapper.Object,
            _passwordHasher.Object,
            _unitOfWork.Object);
    }

    private void SetupMapperAndPasswordHasher(RegisterUserCommand command, User domainUser, UserModel userModel, string hashedPassword = "HashedPassword123!")
    {
        // Setup mapper: RegisterUserCommand -> User (Domain)
        _mapper
            .Setup(x => x.Map<User>(command))
            .Returns(domainUser);

        // Setup mapper: User (Domain) -> UserModel (Infrastructure)
        _mapper
            .Setup(x => x.Map<UserModel>(It.Is<User>(u => u.Id == domainUser.Id)))
            .Returns(userModel);

        // Setup password hasher
        _passwordHasher
            .Setup(x => x.HashPassword(It.IsAny<UserModel>(), command.password))
            .Returns(hashedPassword);
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
            UserRole.Client);

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

        SetupMapperAndPasswordHasher(command, domainUser, userModel);

        _repository
            .Setup(x => x.AddAsync(It.IsAny<User>(), default))
            .Returns(Task.CompletedTask);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _repository.Verify(
            x => x.AddAsync(
                It.Is<User>(u =>
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
            UserRole.Client);

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

        SetupMapperAndPasswordHasher(command, domainUser, userModel);

        Domain.Entities.User? capturedUser = null;
        _repository
            .Setup(x => x.AddAsync(It.IsAny<User>(), default))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user)
            .Returns(Task.CompletedTask);

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

        SetupMapperAndPasswordHasher(command, domainUser, userModel);

        User? capturedUser = null;
        _repository
            .Setup(x => x.AddAsync(It.IsAny<User>(), default))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user)
            .Returns(Task.CompletedTask);

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

        _mapper
            .Setup(x => x.Map<User>(command))
            .Returns(domainUser);

        _repository
            .Setup(x => x.AddAsync(It.IsAny<User>(), default))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database connection failed");
    }
}