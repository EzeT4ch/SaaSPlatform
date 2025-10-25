using AutoMapper;
using TenantModel = AuthService.Infrastructure.Database.Entities.Tenant;

namespace AuthService.Infrastructure.Database.Profiles;

public class TenantMappingProfile : Profile
{
    public TenantMappingProfile()
    {
        CreateMap<TenantModel, Domain.Entities.Tenant>().ReverseMap();
    }
}