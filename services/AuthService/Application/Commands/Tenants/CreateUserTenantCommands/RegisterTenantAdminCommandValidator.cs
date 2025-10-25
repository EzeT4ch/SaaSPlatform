using FluentValidation;

namespace AuthService.Application.Commands.Tenants.CreateUserTenantCommands;

internal sealed class RegisterTenantAdminCommandValidator : AbstractValidator<RegisterTenantAdminCommand>
{
    public RegisterTenantAdminCommandValidator()
    {
        RuleFor(x => x.TenantName)
            .NotEmpty().WithMessage("Tenant name is required.")
            .MaximumLength(100).WithMessage("Tenant name must not exceed 100 characters.");
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(64).WithMessage("Username must not exceed 64 characters.");
    }
}

