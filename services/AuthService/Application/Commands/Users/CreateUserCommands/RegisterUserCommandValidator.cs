using AuthService.Infrastructure.Database.Entities;
using AuthService.Infrastructure.Database.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using User = AuthService.Domain.Entities.User;
using UserModel = AuthService.Infrastructure.Database.Entities.User;
using TenantModel = AuthService.Infrastructure.Database.Entities.Tenant;

namespace AuthService.Application.Commands.Users.CreateUserCommands;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    private readonly IRepository<UserModel, User> _repository;
    private readonly IRepository<TenantModel, Domain.Entities.Tenant> _tenantRepository;
    private readonly RoleManager<Role> _roleManager;

    public RegisterUserCommandValidator(IRepository<UserModel, User> repository, IRepository<TenantModel, Domain.Entities.Tenant> tenantRepository, RoleManager<Role> roleManager)
    {
        _repository = repository;
        _tenantRepository = tenantRepository;
        _roleManager = roleManager;

        RuleFor(x => x.email)
            .MustAsync(BeUniqueEmail)
            .WithMessage("Email address is already in use")
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email is not valid.");

        RuleFor(x => x.password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.");

        RuleFor(x => x.username)
            .NotEmpty()
            .WithMessage("Username is required.")
            .MaximumLength(64)
            .WithMessage("Username must not exceed 64 characters.");

        RuleFor(x => x.fullName)
            .NotEmpty()
            .WithMessage("Full name is required.")
            .MaximumLength(100)
            .WithMessage("Full name must not exceed 100 characters.");

        RuleFor(x => x.tenantId)
            .NotEmpty()
            .WithMessage("TenantId is required.")
            .MustAsync(ExistTenant)
            .WithMessage("Tenant does not exist.");

        RuleFor(x => x.tenantName)
            .MaximumLength(100)
            .WithMessage("Tenant name must not exceed 100 characters.");

        RuleFor(x => x.role)
            .NotEmpty()
            .WithMessage("Role value is invalid.")
            .MustAsync(async (string name, CancellationToken cToken)
                => await BeValidRole(name))
            .WithMessage("Role value is not valid.");
    }

    private async Task<bool> BeUniqueEmail(
        RegisterUserCommand command,
        string email,
        CancellationToken cancellationToken)
        => !await _repository.AsQueryable()
            .AnyAsync(u => u.Email == email && u.TenantId == command.tenantId,
                cancellationToken);


    private async Task<bool> ExistTenant(
        RegisterUserCommand command,
        Guid tenantId,
        CancellationToken cancellationToken)
        => await _tenantRepository.AsQueryable()
            .AnyAsync(u => u.Id == tenantId,
                cancellationToken);

    private async Task<bool> BeValidRole(string roleName)
        => await _roleManager.RoleExistsAsync(roleName);
}