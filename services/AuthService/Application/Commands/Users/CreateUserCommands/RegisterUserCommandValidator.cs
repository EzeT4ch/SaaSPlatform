using AuthService.Domain.Entities;
using AuthService.Infrastructure.Database.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UserModel = AuthService.Infrastructure.Database.Entities.User;

namespace AuthService.Application.Commands.Tenant.CreateUserCommands;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    private readonly IRepository<UserModel, User> _repository;
    public RegisterUserCommandValidator(IRepository<UserModel, User> repository)
    {
        _repository = repository;

        RuleFor(x => x.email)
            .MustAsync(BeUniqueEmail).WithMessage("Unique email")
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");

        RuleFor(x => x.password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");

        RuleFor(x => x.username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(64).WithMessage("Username must not exceed 64 characters.");

        RuleFor(x => x.fullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");

        RuleFor(x => x.tenantId)
            .NotEmpty().WithMessage("TenantId is required.");

        RuleFor(x => x.tenantName)
            .MaximumLength(100).WithMessage("Tenant name must not exceed 100 characters.");

        RuleFor(x => x.role)
            .IsInEnum().WithMessage("Role value is invalid.");
    }

    private async Task<bool> BeUniqueEmail(
        RegisterUserCommand command,
        string email,
        CancellationToken cancellationToken)
    {
        return !await _repository.AsQueryable()
            .AnyAsync(u => u.Email == email && u.TenantId == command.tenantId,
                      cancellationToken);
    }
}

