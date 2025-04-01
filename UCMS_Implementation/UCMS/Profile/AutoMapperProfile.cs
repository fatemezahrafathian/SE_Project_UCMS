using UCMS.DTOs.AuthDto;
using UCMS.Models;

namespace UCMS.Profile;
using AutoMapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<RegisterDto, User>()
            .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForSourceMember(src => src.ConfirmPassword, opt => opt.DoNotValidate());
    }
}