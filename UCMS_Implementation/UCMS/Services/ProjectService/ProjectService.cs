using AutoMapper;
using UCMS.DTOs;
using UCMS.DTOs.ClassDto;
using UCMS.DTOs.ProjectDto;
using UCMS.Factories;
using UCMS.Models;
using UCMS.Repositories.ClassRepository.Abstraction;
using UCMS.Repositories.ProjectRepository.Abstarction;
using UCMS.Repositories.UserRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.ClassService;
using UCMS.Services.ClassService.Abstraction;
using UCMS.Services.FileService;
using UCMS.Services.ImageService;
using UCMS.Services.PasswordService.Abstraction;

namespace UCMS.Services.ProjectService;

public class ProjectService: IProjectService
{
    private readonly IProjectRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly IClassRepository _classRepository;
    private readonly IFileService _fileService;

    public ProjectService(IProjectRepository repository, IHttpContextAccessor httpContextAccessor, IMapper mapper, IClassRepository classRepository, IFileService fileService)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _classRepository = classRepository;
        _fileService = fileService;
    }

    public async Task<ServiceResponse<GetProjectForInstructorDto>> CreateProjectAsync(int classId, CreateProjectDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var currentClass = await _classRepository.GetClassByIdAsync(classId);
        if (currentClass == null)
        {
            return ServiceResponseFactory.Failure<GetProjectForInstructorDto>(Messages.ClassNotFound);
        }
        if (currentClass.InstructorId != user.Instructor.Id)
        {
            return ServiceResponseFactory.Failure<GetProjectForInstructorDto>(Messages.InvalidnIstructorForThisClass);
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

        return ServiceResponseFactory.Success(projectDto, Messages.ClassCreatedSuccessfully); 
    }
    public async Task<ServiceResponse<GetProjectForInstructorDto>> UpdateProjectAsync(int classId, int projectId, PatchProjectDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var currentClass = await _classRepository.GetClassByIdAsync(classId);

        if (currentClass == null)
            return ServiceResponseFactory.Failure<GetProjectForInstructorDto>(Messages.ClassNotFound);
    
        if (currentClass.InstructorId != user.Instructor.Id)
            return ServiceResponseFactory.Failure<GetProjectForInstructorDto>(Messages.InvalidnIstructorForThisClass);
    
        var existingProject = await _repository.GetProjectByIdAsync(projectId);
        if (existingProject == null || existingProject.ClassId != classId)
            return ServiceResponseFactory.Failure<GetProjectForInstructorDto>(Messages.ProjectNotFound);

        var validator = new UpdateProjectDtoValidator(_fileService);
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
            return ServiceResponseFactory.Failure<GetProjectForInstructorDto>(result.Errors.First().ErrorMessage);

        _mapper.Map(dto, existingProject); // map changes
        if (dto.ProjectFile != null)
        {
            _fileService.DeleteFile(existingProject.ProjectFilePath); 
            existingProject.ProjectFilePath = await _fileService.SaveFileAsync(dto.ProjectFile, "projects"); 
        }
           

        existingProject.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(existingProject);

        var projectDto = _mapper.Map<GetProjectForInstructorDto>(existingProject);
        return ServiceResponseFactory.Success(projectDto, Messages.ProjectUpdatedSuccessfully);
    }

}

