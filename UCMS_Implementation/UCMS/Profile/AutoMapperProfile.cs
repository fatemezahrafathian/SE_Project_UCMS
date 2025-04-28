using UCMS.DTOs;
using UCMS.DTOs.AuthDto;
using UCMS.DTOs.ClassDto;
using UCMS.DTOs.RoleDto;
using UCMS.Models;

namespace UCMS.Profile;
using AutoMapper;
using UCMS.DTOs.Instructor;
using UCMS.DTOs.Student;
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

        CreateMap<ClassScheduleDto, ClassSchedule>();
        CreateMap<ClassSchedule, ClassScheduleDto>();

        CreateMap<CreateClassDto, Class>()
            .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => src.Schedules));

        // first mapper
        CreateMap<Class, GetClassDto>()
            .ForMember(dest => dest.InstructorFullName, opt => opt.Ignore());

        CreateMap<Class, GetClassForInstructorDto>()
            .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => src.Schedules));

        CreateMap<Class, GetClassForStudentDto>();

        // second mapper
        CreateMap<Class, GetClassDto>()
            .ForMember(dest => dest.InstructorFullName,
                opt => opt.MapFrom(src => $"{src.Instructor.User.FirstName} {src.Instructor.User.LastName}"))
            .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => src.Schedules));
        // .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate?.ToString("yyyy-MM-dd")));

        //CreateMap<Class, GetClassPreviewDto>()
        //.ForMember(dest => dest.InstructorFullName,
        //opt => opt.MapFrom(src => $"{src.Instructor.User.FirstName} {src.Instructor.User.LastName}"))
        //.ForMember(dest => dest.Schedules,
        //opt => opt.MapFrom(src => src.Schedules));

        //CreateMap<UpdateClassDto, Class>();

        // CreateMap<Class, GetClassPreviewDto>()
        //     .ForMember(dest => dest.InstructorFullName,
        //         opt => opt.MapFrom(src => $"{src.Instructor.User.FirstName} {src.Instructor.User.LastName}"))
        //     .ForMember(dest => dest.Schedules,
        //         opt => opt.MapFrom(src => src.Schedules));

        // see if it works
        CreateMap<PatchClassDto, Class>()
            .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => src.Schedules));

        CreateMap<EditStudentDto, Student>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Student, GetStudentDto>()
            .ForMember(dest => dest.EducationLevel, opt => opt.MapFrom(src => src.EducationLevel.ToString()))
            .ForMember(dest => dest.University, opt => opt.MapFrom(src => src.User.University.ToString()));

        CreateMap<Student, StudentPreviewDto>()
            .ForMember(dest => dest.EducationLevel, opt => opt.MapFrom(src => src.EducationLevel.ToString()))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.ProfileImagePath, opt => opt.MapFrom(src => src.User.ProfileImagePath));

        CreateMap<Student, StudentProfileDto>()
            .ForMember(dest => dest.EducationLevel, opt => opt.MapFrom(src => src.EducationLevel.ToString()))
            .ForMember(dest => dest.University, opt => opt.MapFrom(src => src.User.University.ToString()))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.User.Bio))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.User.Role));

        CreateMap<EditInstructorDto, Instructor>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Instructor, GetInstructorDto>()
            .ForMember(dest => dest.Rank, opt => opt.MapFrom(src => src.Rank.ToString()))
            .ForMember(dest => dest.University, opt => opt.MapFrom(src => src.User.University.ToString()));

        CreateMap<Instructor, InstructorProfileDto>()
            .ForMember(dest => dest.Rank, opt => opt.MapFrom(src => src.Rank.ToString()))
            .ForMember(dest => dest.University, opt => opt.MapFrom(src => src.User.University.ToString()))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.User.Bio))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.User.Role));


        CreateMap<User, OutputUserDto>()
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()))
            .ForMember(dest => dest.University, opt => opt.MapFrom(src => src.University.ToString()));

        CreateMap<EditUserDto, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Class, GetClassPreviewForInstructorDto>(); // calculate student count

        CreateMap<Page<Class>, GetClassPageDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<Student, GetStudentsOfClassforInstructorDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.ProfileImagePath, opt => opt.MapFrom(src => src.User.ProfileImagePath))
            .ForMember(dest => dest.StudentNumber, opt => opt.MapFrom(src => src.StudentNumber));

        CreateMap<Student, GetStudentsOfClassforStudentDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.ProfileImagePath, opt => opt.MapFrom(src => src.User.ProfileImagePath));
    }
}