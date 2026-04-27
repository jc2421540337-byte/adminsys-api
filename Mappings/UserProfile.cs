using AutoMapper;
using NewAdminSystem.Api.DTOs.Auth;
using NewAdminSystem.Api.DTOs.Users;
using NewAdminSystem.Api.Models;

namespace NewAdminSystem.Api.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>();

        CreateMap<UserCreateDto, User>();

        CreateMap<UserUpdateDto, User>();

        CreateMap<RegisterDto, User>();

        CreateMap<User, UserListDto>();

    }
}
