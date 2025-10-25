using AutoMapper;
using AuthService.Domain.Entities;
using UserModel = AuthService.Infrastructure.Database.Entities.User;

namespace AuthService.Infrastructure.Database.Profiles;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<UserModel, User>().ReverseMap();
    }
}