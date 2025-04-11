using UCMS.DTOs.AuthDto;
using UCMS.DTOs.ClassDto;
using UCMS.DTOs.RoleDto;
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
        
        CreateMap<Role, GetRoleDto>();
        
        CreateMap<ClassScheduleDto, ClassSchedule>();
        CreateMap<ClassSchedule, ClassScheduleDto>();

        CreateMap<CreateClassDto, Class>()
            .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => src.Schedules));
        
        // first mapper
        CreateMap<Class, GetClassDto>()
            .ForMember(dest => dest.InstructorFullName, opt => opt.Ignore())
            .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => src.Schedules));
        
        // second mapper
        CreateMap<Class, GetClassDto>()
            .ForMember(dest => dest.InstructorFullName,
                opt => opt.MapFrom(src => $"{src.Instructor.User.FirstName} {src.Instructor.User.LastName}"))
            .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => src.Schedules));
        // .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate?.ToString("yyyy-MM-dd")));
        
        CreateMap<Class, GetClassPreviewDto>()
            .ForMember(dest => dest.InstructorFullName,
                opt => opt.MapFrom(src => $"{src.Instructor.User.FirstName} {src.Instructor.User.LastName}"))
            .ForMember(dest => dest.Schedules,
                opt => opt.MapFrom(src => src.Schedules));
        
        CreateMap<UpdateClassDto, Class>()
            .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => src.Schedules));
    }
}