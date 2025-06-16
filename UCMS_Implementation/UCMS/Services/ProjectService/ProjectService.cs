using AutoMapper;
using UCMS.DTOs;
using UCMS.DTOs.ProjectDto;
using UCMS.Factories;
using UCMS.Models;
using UCMS.Repositories.ClassRepository.Abstraction;
using UCMS.Repositories.ProjectRepository.Abstarction;
using UCMS.Resources;
using UCMS.Services.FileService;

namespace UCMS.Services.ProjectService;

public class ProjectService: IProjectService
{
    private readonly IProjectRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly IClassRepository _classRepository;
    private readonly IStudentClassRepository _studentClassRepository;
    private readonly IFileService _fileService;
    

    public ProjectService(IProjectRepository repository, IHttpContextAccessor httpContextAccessor, IMapper mapper, IClassRepository classRepository, IFileService fileService,IStudentClassRepository studentClassRepository)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _classRepository = classRepository;
        _studentClassRepository = studentClassRepository;
        _fileService = fileService;
    }

    public async Task<ServiceResponse<GetProjectForInstructorDto>> CreateProjectAsync(int classId, CreateProjectDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var currentClass = await _classRepository.GetClassByIdAsync(classId);
        if (currentClass == null)
        {
            return ServiceResponseFactory.Failure<GetProjectForInstructorDto>(Messages.ProjectNotFound);
        }
        if (currentClass.InstructorId != user!.Instructor!.Id)
        {
            return ServiceResponseFactory.Failure<GetProjectForInstructorDto>(Messages.InvalidIstructorForThisClass);
        }
        // var tehranZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");
        //
        // dto.StartDate = TimeZoneInfo.ConvertTimeToUtc(
        //     DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Unspecified),
        //     tehranZone
        // );
        //
        // dto.EndDate = TimeZoneInfo.ConvertTimeToUtc(
        //     DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Unspecified),
        //     tehranZone
        // );
        DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Unspecified);
        DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Unspecified);
        if (currentClass.StartDate.HasValue)
        {
            if (currentClass.StartDate.Value > DateOnly.FromDateTime(dto.StartDate.Date))
            {
                return ServiceResponseFactory.Failure<GetProjectForInstructorDto>(Messages.ProjectStartDateCannotBeBeforeClassStartDate);
            }
        }
        if (currentClass.EndDate.HasValue)
        {
            if (currentClass.EndDate.Value < DateOnly.FromDateTime(dto.EndDate.Date))
            {
                return ServiceResponseFactory.Failure<GetProjectForInstructorDto>(Messages.ProjectEndDateCannotBeAfterClassEndDate);
            }
        }
        if (await _repository.IsProjectNameDuplicateAsync(classId, dto.Title))
        {
            return ServiceResponseFactory.Failure<GetProjectForInstructorDto>(Messages.titleIsDuplicated);
        }
        
        var validator = new CreateProjectDtoValidator(_fileService);
        
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errorMessage = result.Errors.First().ErrorMessage;
            return ServiceResponseFactory.Failure<GetProjectForInstructorDto>(errorMessage);
        }
        string? filePath = null;
        if (dto.ProjectFile != null)
        {
            filePath = await _fileService.SaveFileAsync(dto.ProjectFile, "projects");
        }
        
        var newProject = _mapper.Map<Project>(dto);
        newProject.ClassId=currentClass.Id;
        newProject.ProjectFilePath = filePath;
        await _repository.AddAsync(newProject);
        var projectDto = _mapper.Map<GetProjectForInstructorDto>(newProject);

        return ServiceResponseFactory.Success(projectDto, Messages.ProjectCreatedSuccessfully); 
    }
    public async Task<ServiceResponse<GetProjectForInstructorDto>> UpdateProjectAsync(int projectId, PatchProjectDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var existingProject = await _repository.GetProjectByIdAsync(projectId);
        if (existingProject == null)
            return ServiceResponseFactory.Failure<GetProjectForInstructorDto>(Messages.ProjectNotFound);
    
        if (existingProject.Class.InstructorId != user!.Instructor!.Id)
            return ServiceResponseFactory.Failure<GetProjectForInstructorDto>(Messages.InvalidIstructorForThisClass);
        
        // var tehranZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");
        
        

        if (dto.StartDate.HasValue)
        {
            // dto.StartDate = TimeZoneInfo.ConvertTimeToUtc(
            //     DateTime.SpecifyKind(dto.StartDate.Value, DateTimeKind.Unspecified),
            //     tehranZone
            // );
            DateTime.SpecifyKind(dto.StartDate.Value, DateTimeKind.Unspecified);
            if (existingProject.Class.StartDate.HasValue)
            {
                if (existingProject.Class.StartDate.Value > DateOnly.FromDateTime(dto.StartDate.Value.Date))
                {
                    return ServiceResponseFactory.Failure<GetProjectForInstructorDto>(Messages.ProjectStartDateCannotBeBeforeClassStartDate);
                }
            }
        }
        if (dto.EndDate.HasValue)
        {
            // dto.EndDate = TimeZoneInfo.ConvertTimeToUtc(
            //     DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Unspecified),
            //     tehranZone
            // );
            DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Unspecified);
            if (existingProject.Class.EndDate.HasValue)
            {
                if (existingProject.Class.EndDate.Value < DateOnly.FromDateTime(dto.EndDate.Value.Date))
                {
                    return ServiceResponseFactory.Failure<GetProjectForInstructorDto>(Messages.ProjectEndDateCannotBeAfterClassEndDate);
                }
            }
        }

        if (dto.Title != null)
        {
            if (await _repository.IsProjectNameDuplicateAsync(existingProject.ClassId, dto.Title))
            {
                return ServiceResponseFactory.Failure<GetProjectForInstructorDto>(Messages.titleIsDuplicated);
            }
        }
        var validator = new UpdateProjectDtoValidator(_fileService);
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
            return ServiceResponseFactory.Failure<GetProjectForInstructorDto>(result.Errors.First().ErrorMessage);

        _mapper.Map(dto, existingProject);
        if (dto.ProjectFile != null)
        {
            if (existingProject.ProjectFilePath != null)
            {
                _fileService.DeleteFile(existingProject.ProjectFilePath); 
            }
            existingProject.ProjectFilePath = await _fileService.SaveFileAsync(dto.ProjectFile, "projects"); 
        }
           

        existingProject.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(existingProject);

        var projectDto = _mapper.Map<GetProjectForInstructorDto>(existingProject);
        return ServiceResponseFactory.Success(projectDto, Messages.ProjectUpdatedSuccessfully);
    }
    public async Task<ServiceResponse<string>> DeleteProjectAsync(int projectId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
    
        var project = await _repository.GetProjectByIdAsync(projectId);
        if (project == null)
            return ServiceResponseFactory.Failure<string>(Messages.ProjectNotFound);

        if (project.Class.InstructorId != user!.Instructor!.Id)
            return ServiceResponseFactory.Failure<string>(Messages.ProjectCantBeAccessed);
        if (project.ProjectFilePath != null)
        {
            _fileService.DeleteFile(project.ProjectFilePath);
        }
        await _repository.DeleteAsync(project);
        return ServiceResponseFactory.Success("Project deleted successfully", Messages.ProjectDeletedSuccessfully);
    }
    public async Task<ServiceResponse<GetProjectForInstructorDto>> GetProjectByIdForInstructorAsync(int projectId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var project = await _repository.GetProjectByIdAsync(projectId);

        if (project == null || project.Class.InstructorId != user?.Instructor?.Id)
        {
            return ServiceResponseFactory.Failure<GetProjectForInstructorDto>(Messages.ProjectCantBeAccessed);
        }

        var dto = _mapper.Map<GetProjectForInstructorDto>(project);
        dto.ProjectStatus = CalculateProjectStatus(dto.StartDate,dto.EndDate);
        dto.ProjectFileContentType = GetContentTypeFromPath(dto.ProjectFilePath);
        return ServiceResponseFactory.Success(dto,Messages.ProjectRetrievedSuccessfully);
    }
    public async Task<ServiceResponse<GetProjectForStudentDto>> GetProjectByIdForStudentAsync(int projectId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var project = await _repository.GetProjectByIdAsync(projectId);

        if (project == null || !await _studentClassRepository.IsStudentOfClassAsync(project.ClassId,user!.Student!.Id))
        {
            return ServiceResponseFactory.Failure<GetProjectForStudentDto>(Messages.ProjectCantBeAccessed);
        }

        var dto = _mapper.Map<GetProjectForStudentDto>(project);
        dto.ProjectStatus = CalculateProjectStatus(dto.StartDate,dto.EndDate);
        dto.ProjectFileContentType = GetContentTypeFromPath(dto.ProjectFilePath);
        return ServiceResponseFactory.Success(dto,Messages.ProjectRetrievedSuccessfully);
    }
    private static string? GetContentTypeFromPath(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return null;
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".doc" => "application/msword",
            _ => "application/octet-stream"
        };
    }

    private static ProjectStatus CalculateProjectStatus(DateTime start, DateTime end)
    {
        var now = DateTime.UtcNow;
        if (now < start) return ProjectStatus.NotStarted;
        if (now >= start && now <= end) return ProjectStatus.InProgress;
        return ProjectStatus.Completed;
    }
    public async Task<ServiceResponse<FileDownloadDto>> HandleDownloadProjectFileAsync(int projectId)
    {
        var project = await _repository.GetProjectByIdAsync(projectId);
        if (project == null || string.IsNullOrWhiteSpace(project.ProjectFilePath))
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileDoesNotExist);
        var dto =await _fileService.DownloadFile(project.ProjectFilePath);
        if (dto==null)
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileDoesNotExist);
        if (project.ProjectFilePath != null)
        {
            dto.ContentType= GetContentTypeFromPath(project.ProjectFilePath);
        }

        return ServiceResponseFactory.Success(dto,Messages.ProjectFileDownloadedSuccessfully);
    }
    public async Task<ServiceResponse<List<GetProjectListForInstructorDto>>> GetProjectsForInstructor(FilterProjectsForInstructorDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var projectEntityList = await _repository.FilterProjectsForInstructorAsync(user!.Instructor!.Id, dto.Title, dto.ClassTitle, dto.ProjectStatus,dto.OrderBy,dto.Descending);        
        
        var responseDto = _mapper.Map<List<GetProjectListForInstructorDto>>(projectEntityList);
        
        return ServiceResponseFactory.Success(responseDto, Messages.ProjectsRetrievedSuccessfully);
    }
    public async Task<ServiceResponse<List<GetProjectListForStudentDto>>> GetProjectsForStudent(FilterProjectsForStudentDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var projectEntityList = await _repository.FilterProjectsForStudentAsync(user!.Student!.Id, dto.Title, dto.ClassTitle, dto.ProjectStatus,dto.OrderBy,dto.Descending);        
        
        var responseDto = _mapper.Map<List<GetProjectListForStudentDto>>(projectEntityList);
        
        return ServiceResponseFactory.Success(responseDto, Messages.ProjectsRetrievedSuccessfully);
    }
    public async Task<ServiceResponse<List<GetProjectsOfClassDto>>> GetProjectsOfClassForInstructorAsync(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var currentClass = await _classRepository.GetClassByIdAsync(classId);
        if (currentClass == null)
        {
            return ServiceResponseFactory.Failure<List<GetProjectsOfClassDto>>(Messages.ClassNotFound);
        }
        if (currentClass.InstructorId != user!.Instructor!.Id)
        {
            return ServiceResponseFactory.Failure<List<GetProjectsOfClassDto>>(Messages.InvalidIstructorForThisClass);
        }
        var projects = await _repository.GetProjectsByClassIdAsync(classId);

        var dto = _mapper.Map<List<GetProjectsOfClassDto>>(projects);
        return ServiceResponseFactory.Success(dto, Messages.ProjectsRetrievedSuccessfully);
    }
    public async Task<ServiceResponse<List<GetProjectsOfClassDto>>> GetProjectsOfClassForStudentAsync(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var currentClass = await _classRepository.GetClassByIdAsync(classId);
        if (currentClass == null)
        {
            return ServiceResponseFactory.Failure<List<GetProjectsOfClassDto>>(Messages.ClassNotFound);
        }
        if (!await _studentClassRepository.IsStudentOfClassAsync(classId, user!.Student!.Id))
        {
            return ServiceResponseFactory.Failure<List<GetProjectsOfClassDto>>(Messages.StudentNotInClass);
        }
        var projects = await _repository.GetProjectsByClassIdAsync(classId);

        var dto = _mapper.Map<List<GetProjectsOfClassDto>>(projects);
        return ServiceResponseFactory.Success(dto, Messages.ProjectsRetrievedSuccessfully);    
    }
}

