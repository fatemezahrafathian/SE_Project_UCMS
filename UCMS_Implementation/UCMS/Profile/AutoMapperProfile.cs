using UCMS.DTOs.AuthDto;
using UCMS.DTOs.ClassDto;
using UCMS.DTOs.RoleDto;
using UCMS.Models;

namespace UCMS.Profile;
using AutoMapper;
using UCMS.DTOs.Student;

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

        CreateMap<EditStudentDto, Student>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Student, GetStudentDto>();

        CreateMap<Student, StudentPreviewDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.ProfileImagePath, opt => opt.MapFrom(src => src.User.ProfileImagePath));

        CreateMap<Student, StudentProfileDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.User.Bio))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.User.Role));

    }
}