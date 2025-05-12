using UCMS.DTOs;
using UCMS.DTOs.AuthDto;
using UCMS.DTOs.ClassDto;
using UCMS.DTOs.ProjectDto;
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
            .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => src.Schedules))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        
        CreateMap<EditStudentDto, Student>()
            .ForPath(dest => dest.User.University, opt => opt.MapFrom(src => src.University))
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
            .ForMember(dest => dest.ProfileImagePath, opt => opt.MapFrom(src => src.User.ProfileImagePath))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.User.Bio))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.User.Role));

        CreateMap<EditInstructorDto, Instructor>()
            .ForPath(dest => dest.User.University, opt => opt.MapFrom(src => src.University))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Instructor, GetInstructorDto>()
            .ForMember(dest => dest.Rank, opt => opt.MapFrom(src => src.Rank.ToString()))
            .ForMember(dest => dest.University, opt => opt.MapFrom(src => src.User.University.ToString()));

        CreateMap<Instructor, InstructorProfileDto>()
            .ForMember(dest => dest.Rank, opt => opt.MapFrom(src => src.Rank.ToString()))
            .ForMember(dest => dest.University, opt => opt.MapFrom(src => src.User.University.ToString()))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.ProfileImagePath, opt => opt.MapFrom(src => src.User.ProfileImagePath))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.User.Bio))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.User.Role));


        CreateMap<User, OutputUserDto>()
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()))
            .ForMember(dest => dest.University, opt => opt.MapFrom(src => src.University.ToString()));

        CreateMap<EditUserDto, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Class, GetClassPreviewForInstructorDto>() // calculate student count
            .ForMember(dest => dest.StudentCount, opt => opt.MapFrom(src => src.ClassStudents.Count));

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

        CreateMap<Page<Class>, GetClassPageForStudentDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
        CreateMap<Class, GetClassPreviewForStudentDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src => src.ProfileImageUrl))
            .ForMember(dest => dest.InstructorFullName,
            opt => opt.MapFrom(src => $"{src.Instructor.User.FirstName} {src.Instructor.User.LastName}"))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.StudentCount, opt => opt.MapFrom(src => src.ClassStudents.Count));
        
        CreateMap<ClassSchedule, ClassScheduleDto>();

        CreateMap<CreateProjectDto, Project>()
            .ForMember(dest => dest.ProjectType, opt => opt.MapFrom(src => (ProjectType)src.ProjectType))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ToUniversalTime()))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.ToUniversalTime()))
            .ForMember(dest => dest.ProjectFilePath, opt => opt.Ignore()) 
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        CreateMap<PatchProjectDto, Project>()
            .ForMember(dest => dest.ProjectFilePath, opt => opt.Ignore())
            // .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) =>
                srcMember != null &&
                !(srcMember is string s && string.IsNullOrWhiteSpace(s)) &&
                !(srcMember is int i && i == 0) &&
                !(srcMember is DateTime dt && dt == default)
            ));
        CreateMap<Project, GetProjectForStudentDto>()
            .ForMember(dest => dest.ProjectFileContentType, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectStatus, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectFilePath, opt => opt.MapFrom(src => src.ProjectFilePath));
        CreateMap<Project, GetProjectForInstructorDto>()
            .ForMember(dest => dest.ProjectFileContentType, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectStatus, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectFilePath, opt => opt.MapFrom(src => src.ProjectFilePath));
        CreateMap<Project, GetProjectListForStudentDto>()
            .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.EndDate.Date))  
            .ForMember(dest => dest.DueTime, opt => opt.MapFrom(src => src.EndDate.TimeOfDay)) 
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.EndDate < DateTime.UtcNow ? ProjectStatus.Completed : (src.StartDate > DateTime.UtcNow ? ProjectStatus.NotStarted : ProjectStatus.InProgress))) 
            .ForMember(dest => dest.ClassTitle, opt => opt.MapFrom(src => src.Class.Title)); 
        CreateMap<Project, GetProjectListForInstructorDto>()
            .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.EndDate.Date))  
            .ForMember(dest => dest.DueTime, opt => opt.MapFrom(src => src.EndDate.TimeOfDay))  
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.EndDate < DateTime.UtcNow ? ProjectStatus.Completed : (src.StartDate > DateTime.UtcNow ? ProjectStatus.NotStarted : ProjectStatus.InProgress)))
            .ForMember(dest => dest.ClassTitle, opt => opt.MapFrom(src => src.Class.Title)); 

    }
}