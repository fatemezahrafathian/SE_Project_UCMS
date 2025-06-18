using AutoMapper;
using UCMS.DTOs;
using UCMS.DTOs.PhaseDto;
using UCMS.Factories;
using UCMS.Models;
using UCMS.Repositories.ClassRepository.Abstraction;
using UCMS.Repositories.PhaseRepository.Abstraction;
using UCMS.Repositories.ProjectRepository.Abstarction;
using UCMS.Resources;
using UCMS.Services.FileService;
using UCMS.Services.PhaseService.Abstraction;

namespace UCMS.Services.PhaseService;

public class PhaseService:IPhaseService
{
    private readonly IPhaseRepository _repository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IProjectRepository _projectRepository;
    private readonly IFileService _fileService;
    private readonly IStudentClassRepository _studentClassRepository;

    public PhaseService(IPhaseRepository repository, IMapper mapper,IHttpContextAccessor httpContextAccessor,IProjectRepository projectRepository,IFileService fileService,IStudentClassRepository studentClassRepository)
    {
        _repository = repository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _projectRepository = projectRepository;
        _fileService = fileService;
        _studentClassRepository = studentClassRepository;
    }

    public async Task<ServiceResponse<GetPhaseForInstructorDto>> CreatePhaseAsync(int projectId,CreatePhaseDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var currentProject = await _projectRepository.GetProjectByIdAsync(projectId);
        if (currentProject == null)
        {
            return ServiceResponseFactory.Failure<GetPhaseForInstructorDto>(Messages.ProjectNotFound);
        }
        if (currentProject.Class.InstructorId != user!.Instructor!.Id)
        {
            return ServiceResponseFactory.Failure<GetPhaseForInstructorDto>(Messages.InvalidIstructorForThisProject);
        }
        if (currentProject.StartDate > dto.StartDate)
        {
            return ServiceResponseFactory.Failure<GetPhaseForInstructorDto>(Messages.PhaseStartTimeCannotBeBeforeProjectStartTime);
        }
        if (currentProject.EndDate < dto.EndDate)
        {
            return ServiceResponseFactory.Failure<GetPhaseForInstructorDto>(Messages.PhaseEndTimeCannotBeAfterProjectEndTime);
        }
        var validator = new CreatePhaseDtoValidator(_fileService);
        var result = await validator.ValidateAsync(dto);
        var existingPhases = await _repository.GetPhasesByProjectIdAsync(currentProject.Id);
        if (existingPhases.Any(p => p.Title.Trim().ToLower() == dto.Title.Trim().ToLower()))
        {
            return ServiceResponseFactory.Failure<GetPhaseForInstructorDto>(Messages.PhaseAlreadyExists);
        }
        string? filePath = null;
        if (dto.PhaseFile != null)
        {
            filePath = await _fileService.SaveFileAsync(dto.PhaseFile, "phases");
        }
        if (!result.IsValid)
        {
            var errorMessage = result.Errors.First().ErrorMessage;
            return ServiceResponseFactory.Failure<GetPhaseForInstructorDto>(errorMessage);
        }
        var newPhase = _mapper.Map<Phase>(dto);
        newPhase.ProjectId=currentProject.Id;
        newPhase.PhaseFilePath = filePath;
        await _repository.AddAsync(newPhase);
        var phaseDto = _mapper.Map<GetPhaseForInstructorDto>(newPhase);
        return ServiceResponseFactory.Success(phaseDto, Messages.PhaseCreatedSuccessfully);
    }

    public async Task<ServiceResponse<GetPhaseForInstructorDto>> GetPhaseByIdForInstructorAsync(int phaseId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var phase = await _repository.GetPhaseByIdAsync(phaseId);
        
        if (phase == null || phase.Project.Class.InstructorId != user?.Instructor?.Id)
        {
            return ServiceResponseFactory.Failure<GetPhaseForInstructorDto>(Messages.PhaseCantBeAccessed);
        }

        var dto = _mapper.Map<GetPhaseForInstructorDto>(phase);

        return ServiceResponseFactory.Success(dto, Messages.PhaseRetrievedSuccessfully);
    }
    public async Task<ServiceResponse<GetPhaseForInstructorDto>> UpdatePhaseAsync(int phaseId, PatchPhaseDto dto)
    {
        
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var existingPhase = await _repository.GetPhaseByIdAsync(phaseId);
        if (existingPhase == null)
            return ServiceResponseFactory.Failure<GetPhaseForInstructorDto>(Messages.PhaseNotFound);

        if (existingPhase.Project.Class.InstructorId != user?.Instructor?.Id)
            return ServiceResponseFactory.Failure<GetPhaseForInstructorDto>(Messages.InvalidIstructorForThisPhase);
        
        // var tehranZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");

        if (dto.StartDate.HasValue)
        {
            // dto.StartDate = TimeZoneInfo.ConvertTimeToUtc(
            //     DateTime.SpecifyKind(dto.StartDate.Value, DateTimeKind.Unspecified),
            //     tehranZone
            // );
            DateTime.SpecifyKind(dto.StartDate.Value, DateTimeKind.Unspecified);
            if (existingPhase.Project.StartDate > dto.StartDate)
            {
                return ServiceResponseFactory.Failure<GetPhaseForInstructorDto>(Messages.PhaseStartTimeCannotBeBeforeProjectStartTime);
            }
        }
        if (dto.EndDate.HasValue)
        {
            // dto.EndDate = TimeZoneInfo.ConvertTimeToUtc(
            //     DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Unspecified),
            //     tehranZone
            // );
            DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Unspecified);
            if (existingPhase.Project.EndDate < dto.EndDate)
            {
                return ServiceResponseFactory.Failure<GetPhaseForInstructorDto>(Messages.PhaseEndTimeCannotBeAfterProjectEndTime);
            }
        }
        var validator = new UpdatePhaseDtoValidator(_fileService);
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return ServiceResponseFactory.Failure<GetPhaseForInstructorDto>(validationResult.Errors.First().ErrorMessage);

        if (dto.Title != null)
        {
            var isDuplicate = await _repository.ExistsWithTitleExceptIdAsync(dto.Title, existingPhase.ProjectId, phaseId);
            if (isDuplicate)
                return ServiceResponseFactory.Failure<GetPhaseForInstructorDto>(Messages.PhaseAlreadyExists);
        }

        _mapper.Map(dto, existingPhase);

        if (dto.PhaseFile != null)
        {
            if (existingPhase.PhaseFilePath != null)
                _fileService.DeleteFile(existingPhase.PhaseFilePath);
            existingPhase.PhaseFilePath = await _fileService.SaveFileAsync(dto.PhaseFile, "phases");
        }

        existingPhase.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(existingPhase);

        var phaseDto = _mapper.Map<GetPhaseForInstructorDto>(existingPhase);
        return ServiceResponseFactory.Success(phaseDto, Messages.PhaseUpdatedSuccessfully);
    }

    public async Task<ServiceResponse<string>>  DeletePhaseAsync(int phaseId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var phase = await _repository.GetPhaseByIdAsync(phaseId);
        if (phase == null)
            return ServiceResponseFactory.Failure<string>(Messages.PhaseNotFound);

        if (phase.Project.Class.InstructorId != user!.Instructor!.Id)
            return ServiceResponseFactory.Failure<string>(Messages.PhaseCantBeAccessed);
        
        await _repository.DeleteAsync(phase);
        if (phase.PhaseFilePath != null) 
            _fileService.DeleteFile(phase.PhaseFilePath!);
        return ServiceResponseFactory.Success("Phase deleted successfully", Messages.ProjectDeletedSuccessfully);
    }
    public async Task<ServiceResponse<List<GetPhasesForInstructorDto>>> GetPhasesForInstructor(int projectId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var project = await _projectRepository.GetProjectByIdAsync(projectId);

        if (project == null || project.Class.InstructorId != user?.Instructor?.Id)
        {
            return ServiceResponseFactory.Failure<List<GetPhasesForInstructorDto>>(Messages.PhaseCantBeAccessed);
        }
        var phases = await _repository.GetPhasesByProjectIdAsync(projectId);
        var dto =  _mapper.Map<List<GetPhasesForInstructorDto>>(phases);
        return ServiceResponseFactory.Success(dto,Messages.PhasesRetrievedSuccessfully);
    }
    public async Task<ServiceResponse<FileDownloadDto>> HandleDownloadPhaseFileForInstructorAsync(int phaseId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var phase = await _repository.GetPhaseByIdAsync(phaseId);
        
        if (phase == null || phase.Project.Class.InstructorId != user?.Instructor?.Id)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.PhaseCantBeAccessed);
        }
        if (string.IsNullOrWhiteSpace(phase.PhaseFilePath))
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileNotFound);
        var dto =await _fileService.DownloadFile(phase.PhaseFilePath);
        if (dto==null)
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileDoesNotExist);
        dto.ContentType = GetContentTypeFromPath(phase.PhaseFilePath);
        return ServiceResponseFactory.Success(dto,Messages.PhaseFileDownloadedSuccessfully);
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
    public async Task<ServiceResponse<GetPhaseForStudentDto>> GetPhaseByIdForStudentAsync(int phaseId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var phase = await _repository.GetPhaseByIdAsync(phaseId);
        
        if (phase == null)
        {
            return ServiceResponseFactory.Failure<GetPhaseForStudentDto>(Messages.PhaseCantBeAccessed);
        }
        
        if (!await _studentClassRepository.IsStudentOfClassAsync(phase.Project.ClassId,user!.Student!.Id))
        {
            return ServiceResponseFactory.Failure<GetPhaseForStudentDto>(Messages.PhaseCantBeAccessed);
        }

        var dto = _mapper.Map<GetPhaseForStudentDto>(phase);

        return ServiceResponseFactory.Success(dto, Messages.PhaseRetrievedSuccessfully);
    }
    public async Task<ServiceResponse<List<GetPhasesForStudentDto>>> GetPhasesForStudent(int projectId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var project = await _projectRepository.GetProjectByIdAsync(projectId);

        if (project == null || !await _studentClassRepository.IsStudentOfClassAsync(project.ClassId,user!.Student!.Id))
        {
            return ServiceResponseFactory.Failure<List<GetPhasesForStudentDto>>(Messages.PhaseCantBeAccessed);
        }
        var phases = await _repository.GetPhasesByProjectIdAsync(projectId);
        var dto =  _mapper.Map<List<GetPhasesForStudentDto>>(phases);
        return ServiceResponseFactory.Success(dto,Messages.PhasesRetrievedSuccessfully);
    }
    public async Task<ServiceResponse<FileDownloadDto>> HandleDownloadPhaseFileForStudentAsync(int phaseId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var phase = await _repository.GetPhaseByIdAsync(phaseId);
        if (phase == null || !await _studentClassRepository.IsStudentOfClassAsync(phase.Project.ClassId,user!.Student!.Id))
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.PhaseCantBeAccessed);
        }
        var project = await _repository.GetPhaseByIdAsync(phaseId);
        if (project == null || string.IsNullOrWhiteSpace(project.PhaseFilePath))
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.PhaseOrFileNotFound);
        var dto =await _fileService.DownloadFile(project.PhaseFilePath);
        if (dto==null)
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileDoesNotExist);
        dto.ContentType = GetContentTypeFromPath(project.PhaseFilePath);
        return ServiceResponseFactory.Success(dto,Messages.PhaseFileDownloadedSuccessfully);
    }
    
}