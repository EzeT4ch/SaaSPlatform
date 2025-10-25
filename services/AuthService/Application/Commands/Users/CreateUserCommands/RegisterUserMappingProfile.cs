using AuthService.Domain.Entities;
using AutoMapper;

namespace AuthService.Application.Commands.Tenant.CreateUserCommands;

internal sealed class RegisterUserMappingProfile : Profile
{
    public RegisterUserMappingProfile()
    {
        CreateMap<RegisterUserCommand, User>()
        .ForMember(d => d.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
        .ForMember(d => d.Email, opt => opt.MapFrom(s => s.email))
        .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.username))
        .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.fullName))
        .ForMember(d => d.TenantId, opt => opt.MapFrom(s => s.tenantId))
        .ForMember(d => d.Role, opt => opt.MapFrom(s => s.role))
        .ForMember(d => d.PasswordHash, opt => opt.Ignore())
        .ForMember(d => d.IsActive, opt => opt.MapFrom(_ => true))
        .ForMember(d => d.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}
