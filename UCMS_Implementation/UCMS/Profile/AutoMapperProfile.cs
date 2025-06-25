using UCMS.DTOs;
using UCMS.DTOs.AuthDto;
using UCMS.DTOs.ClassDto;
using UCMS.DTOs.ExerciseDto;
using UCMS.DTOs.PhaseDto;
using UCMS.DTOs.ExamDto;
using UCMS.DTOs.ExerciseSubmissionDto;
using UCMS.DTOs.PhaseSubmissionDto;
using UCMS.DTOs.ProjectDto;
using UCMS.DTOs.RoleDto;
using UCMS.DTOs.TeamDto;
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
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.ProfileImagePath, opt => opt.MapFrom(src => src.User.ProfileImagePath))
            .ForMember(dest => dest.StudentNumber, opt => opt.MapFrom(src => src.StudentNumber));

        CreateMap<Student, GetStudentsOfClassforStudentDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
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
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
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
        CreateMap<Project, GetProjectsOfClassDto>()
            .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.EndDate.Date))
            .ForMember(dest => dest.DueTime, opt => opt.MapFrom(src => src.EndDate.TimeOfDay))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src =>
                    src.EndDate < DateTime.UtcNow
                        ? ProjectStatus.Completed
                        : (src.StartDate > DateTime.UtcNow ? ProjectStatus.NotStarted : ProjectStatus.InProgress)));
        CreateMap<CreateExerciseDto, Exercise>()
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.ExerciseFilePath, opt => opt.Ignore()) // File should be handled separately
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        CreateMap<Exercise, GetExerciseForInstructorDto>()
            .ForMember(dest => dest.exerciseId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ExerciseFilePath, opt => opt.MapFrom(src => src.ExerciseFilePath));
        CreateMap<PatchExerciseDto, Exercise>()
            .ForMember(dest => dest.ExerciseFilePath, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ClassId, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) =>
                srcMember != null &&
                !(srcMember is string s && string.IsNullOrWhiteSpace(s)) &&
                !(srcMember is int i && i == 0) &&
                !(srcMember is DateTime dt && dt == default)
            ));
        CreateMap<Exercise, GetExercisesForInstructorDto>()
            .ForMember(dest => dest.exerciseId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.classTitle, opt => opt.MapFrom(src => src.Class.Title))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.Status, 
                opt => opt.MapFrom(src => calculateExerciseStatus(src.StartDate, src.EndDate)));
        CreateMap<CreatePhaseDto, Phase>()
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.PhaseFilePath, opt => opt.Ignore()) // File should be handled separately
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        CreateMap<Phase, GetPhaseForInstructorDto>()
            .ForMember(dest => dest.phaseId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.PhaseFilePath, opt => opt.MapFrom(src => src.PhaseFilePath));
        CreateMap<PatchPhaseDto, Phase>()
            .ForMember(dest => dest.PhaseFilePath, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) =>
                srcMember != null &&
                !(srcMember is string s && string.IsNullOrWhiteSpace(s)) &&
                !(srcMember is int i && i == 0) &&
                !(srcMember is DateTime dt && dt == default)
            ));
        CreateMap<Phase, GetPhasesForInstructorDto>()
            .ForMember(dest => dest.phaseId, opt => opt.MapFrom(src => src.Id));
        CreateMap<Phase, GetPhaseForStudentDto>()
            .ForMember(dest => dest.phaseId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.PhaseFilePath, opt => opt.MapFrom(src => src.PhaseFilePath));
        CreateMap<Phase, GetPhasesForStudentDto>()
            .ForMember(dest => dest.phaseId, opt => opt.MapFrom(src => src.Id));

        CreateMap<Exercise, GetExerciseForStudentDto>()
            .ForMember(dest => dest.exerciseId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ExerciseFilePath, opt => opt.MapFrom(src => src.ExerciseFilePath));
        CreateMap<Exercise, GetExercisesForStudentDto>()
            .ForMember(dest => dest.exerciseId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.classTitle, opt => opt.MapFrom(src => src.Class.Title))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.Status, 
                opt => opt.MapFrom(src => calculateExerciseStatus(src.StartDate, src.EndDate)));
        CreateMap<CreateTeamDto, Team>()
            .ForMember(dest => dest.StudentTeams, opt => opt.Ignore());
        
        CreateMap<Team, GetTeamForInstructorDto>();
        CreateMap<Team, GetTeamForStudentDto>();
        CreateMap<Team, GetTeamPreviewDto>();
        
        CreateMap<StudentTeam, GetStudentTeamForInstructorDto>()
            .ForMember(dest => dest.StudentNumber, opt => opt.MapFrom(src => src.Student.StudentNumber))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.Student.User.FirstName} {src.Student.User.LastName}"));
        CreateMap<StudentTeam, GetStudentTeamForStudentDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.Student.User.FirstName} {src.Student.User.LastName}"));
        
        CreateMap<PatchTeamDto, Team>()
            .ForMember(dest => dest.Name,
                opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.Name)));
        CreateMap<CreateExamDto, Exam>()
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        CreateMap<Exam, GetExamForInstructorDto>()
            .ForMember(dest => dest.ExamId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.classTitle, opt => opt.MapFrom(src => src.Class.Title))
            .ForMember(dest => dest.ExamLocation, opt => opt.MapFrom(src => src.ExamLocation))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
            .ForMember(dest => dest.ExamScore, opt => opt.MapFrom(src => src.ExamScore));
        CreateMap<PatchExamDto, Exam>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ClassId, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) =>
                srcMember != null &&
                !(srcMember is string s && string.IsNullOrWhiteSpace(s)) &&
                !(srcMember is int i && i == 0) &&
                !(srcMember is DateTime dt && dt == default)
            ));
        CreateMap<Exam, GetExamForStudentDto>()
            .ForMember(dest => dest.examId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.classTitle, opt => opt.MapFrom(src => src.Class.Title))
            .ForMember(dest => dest.ExamLocation, opt => opt.MapFrom(src => src.ExamLocation))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
            .ForMember(dest => dest.ExamScore, opt => opt.MapFrom(src => src.ExamScore));

        CreateMap<CreatePhaseSubmissionDto, PhaseSubmission>();

        CreateMap<PhaseSubmission, GetPhaseSubmissionPreviewForInstructorDto>()
            .ForMember(dest => dest.TeamId, opt => opt.MapFrom(src => src.StudentTeamPhase.StudentTeam.TeamId))
            .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.StudentTeamPhase.StudentTeam.Team.Name))
            .ForMember(dest => dest.FileType, opt => opt.Ignore());
        
        CreateMap<PhaseSubmission, GetPhaseSubmissionPreviewForStudentDto>()
            .ForMember(dest => dest.FileType, opt => opt.Ignore());

        CreateMap<CreateExerciseSubmissionDto, ExerciseSubmission>();
        
        CreateMap<ExerciseSubmission, GetExerciseSubmissionPreviewForInstructorDto>()
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src =>  $"{src.Student.User.LastName} {src.Student.User.FirstName}"))
            .ForMember(dest => dest.StudentNumber, opt => opt.MapFrom(src =>  src.Student.StudentNumber))
            .ForMember(dest => dest.FileType, opt => opt.Ignore());
        
        CreateMap<ExerciseSubmission, GetExerciseSubmissionPreviewForStudentDto>()
            .ForMember(dest => dest.FileType, opt => opt.Ignore());

        CreateMap<StudentTeamPhase, GetStudentTeamPhasePreviewDto>()
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => $"{src.StudentTeam.Student.User.LastName} {src.StudentTeam.Student.User.FirstName}"))
            .ForMember(dest => dest.StudentNumber, opt => opt.MapFrom(src => src.StudentTeam.Student.StudentNumber));
    }
    private static ExerciseStatus calculateExerciseStatus(DateTime start, DateTime end)
    {
        var now = DateTime.UtcNow;
        if (now < start) return ExerciseStatus.NotStarted;
        if (now >= start && now <= end) return ExerciseStatus.InProgress;
        return ExerciseStatus.Completed;
    }
}