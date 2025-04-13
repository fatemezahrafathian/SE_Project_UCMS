using UCMS.DTOs.AuthDto;
using UCMS.DTOs.RoleDto;
using UCMS.Models;

namespace UCMS.Profile;
using AutoMapper;
using UCMS.DTOs.User;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<RegisterDto, User>()
            .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForSourceMember(src => src.ConfirmPassword, opt => opt.DoNotValidate());
        
        CreateMap<Role, GetRoleDto>();

        CreateMap<User, OutputUserDto>();
    }
}