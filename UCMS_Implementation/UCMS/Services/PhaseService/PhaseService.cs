using AutoMapper;
using Microsoft.AspNetCore.StaticFiles;
using UCMS.DTOs;
using UCMS.DTOs.PhaseDto;
using UCMS.DTOs.ProjectDto;
using UCMS.Factories;
using UCMS.Models;
using UCMS.Repositories.ClassRepository.Abstraction;
using UCMS.Repositories.PhaseRepository.Abstraction;
using UCMS.Repositories.PhaseSubmissionRepository.Abstraction;
using UCMS.Repositories.ProjectRepository.Abstarction;
using UCMS.Repositories.StudentTeamPhaseRepository.Abstraction;
using UCMS.Repositories.TeamRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.FileService;
using UCMS.Services.PhaseService.Abstraction;

namespace UCMS.Services.PhaseService;

public class PhaseService:IPhaseService
{
    private readonly IPhaseRepository _repository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IClassRepository _classRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IFileService _fileService;
    private readonly IStudentClassRepository _studentClassRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IPhaseSubmissionRepository _phaseSubmissionRepository;
    private readonly IStudentTeamPhaseRepository _studentTeamPhaseRepository;

    public PhaseService(IPhaseRepository repository, IMapper mapper,IHttpContextAccessor httpContextAccessor,IClassRepository classRepository,IProjectRepository projectRepository,IFileService fileService,IStudentClassRepository studentClassRepository, ITeamRepository teamRepository, IPhaseSubmissionRepository phaseSubmissionRepository, IStudentTeamPhaseRepository studentTeamPhaseRepository)
    {
        _repository = repository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _classRepository = classRepository;
        _projectRepository = projectRepository;
        _fileService = fileService;
        _studentClassRepository = studentClassRepository;
        _teamRepository = teamRepository;
        _phaseSubmissionRepository = phaseSubmissionRepository;
        _studentTeamPhaseRepository = studentTeamPhaseRepository;
    }

    public async Task<ServiceResponse<GetPhaseForInstructorDto>> CreatePhaseAsync(int projectId,CreatePhaseDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var currentProject = await _projectRepository.GetProjectByIdAsync(projectId);
        if (currentProject == null)
        {
            return ServiceResponseFactory.Failure<GetPhaseForInstructorDto>(Messages.ProjectNotFound);
        }
        var currentClass = currentProject.Class;
        if (currentClass == null)
        {
            return ServiceResponseFactory.Failure<GetPhaseForInstructorDto>(Messages.ClassNotFound);
        }
        if (currentClass.InstructorId != user.Instructor.Id)
        {
            return ServiceResponseFactory.Failure<GetPhaseForInstructorDto>(Messages.InvalidIstructorForThisClass);
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

        var newStudentTeamPhases = new List<StudentTeamPhase>();
        var teams = await _teamRepository.GetTeamsWithRelationsByProjectIdAsync(newPhase.ProjectId); // to be done on active teams
        foreach (var team in teams)
        {
            foreach (var stdTeam in team.StudentTeams)
            {
                var newStudentTeamPhase = new StudentTeamPhase()
                {
                    StudentTeamId = stdTeam.Id,
                    PhaseId = newPhase.Id
                };

                newStudentTeamPhases.Add(newStudentTeamPhase);
            }
        }
        await _studentTeamPhaseRepository.AddRangeStudentTeamPhaseAsync(newStudentTeamPhases);

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
    public async Task<ServiceResponse<GetPhaseForInstructorDto>> UpdatePhaseAsync(int projectId, int phaseId, PatchPhaseDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var currentProject = await _projectRepository.GetProjectByIdAsync(projectId);
        if (currentProject == null)
            return ServiceResponseFactory.Failure<GetPhaseForInstructorDto>(Messages.ProjectNotFound);

        if (currentProject.Class == null || currentProject.Class.InstructorId != user?.Instructor?.Id)
            return ServiceResponseFactory.Failure<GetPhaseForInstructorDto>(Messages.InvalidIstructorForThisClass);

        var existingPhase = await _repository.GetPhaseByIdAsync(phaseId);
        if (existingPhase == null || existingPhase.ProjectId != projectId)
            return ServiceResponseFactory.Failure<GetPhaseForInstructorDto>(Messages.PhaseNotFound);

        var validator = new UpdatePhaseDtoValidator(_fileService);
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return ServiceResponseFactory.Failure<GetPhaseForInstructorDto>(validationResult.Errors.First().ErrorMessage);
        
        var isDuplicate = await _repository.ExistsWithTitleExceptIdAsync(dto.Title, projectId, phaseId);
        if (isDuplicate)
            return ServiceResponseFactory.Failure<GetPhaseForInstructorDto>(Messages.PhaseAlreadyExists);

        _mapper.Map(dto, existingPhase);

        if (dto.PhaseFile != null)
        {
            _fileService.DeleteFile(existingPhase.PhaseFilePath);
            existingPhase.PhaseFilePath = await _fileService.SaveFileAsync(dto.PhaseFile, "phases");
        }

        existingPhase.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(existingPhase);

        var phaseDto = _mapper.Map<GetPhaseForInstructorDto>(existingPhase);
        return ServiceResponseFactory.Success(phaseDto, Messages.PhaseUpdatedSuccessfully);
    }

    public async Task<ServiceResponse<string>>  DeletePhaseAsync(int projectId, int phaseId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
    
        var project = await _projectRepository.GetProjectByIdAsync(projectId);
        if (project == null)
            return ServiceResponseFactory.Failure<string>(Messages.ProjectNotFound);

        if (project.Class.InstructorId != user!.Instructor!.Id)
            return ServiceResponseFactory.Failure<string>(Messages.PhaseCantBeAccessed);
        
        var phase = await _repository.GetPhaseByIdAsync(phaseId);
        if (phase == null)
            return ServiceResponseFactory.Failure<string>(Messages.PhaseNotFound);
        if (phase.ProjectId != projectId)
        {
            return ServiceResponseFactory.Failure<string>(Messages.PhaseCantBeAccessed);
        }
        await _repository.DeleteAsync(phase);
        _fileService.DeleteFile(project.ProjectFilePath);
        return ServiceResponseFactory.Success("Phase deleted successfully", Messages.ProjectDeletedSuccessfully);
    }
    public async Task<ServiceResponse<List<GetPhasesForInstructorDto>>> GetPhasesForInstructor(int projectId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var project = await _projectRepository.GetProjectByIdAsync(projectId);

        if (project == null || project.Class.InstructorId != user?.Instructor?.Id)
        {
            return ServiceResponseFactory.Failure<List<GetPhasesForInstructorDto>>(Messages.ProjectCantBeAccessed);
        }
        var phases = await _repository.GetPhasesByProjectIdAsync(projectId);
        var dto =  _mapper.Map<List<GetPhasesForInstructorDto>>(phases);
        return ServiceResponseFactory.Success(dto,Messages.PhasesRetrievedSuccessfully);
    }
    public async Task<ServiceResponse<FileDownloadDto>> HandleDownloadPhaseFileForInstructorAsync(int phaseId)
    {
        //check access for instructor and student
        var project = await _repository.GetPhaseByIdAsync(phaseId);
        if (project == null || string.IsNullOrWhiteSpace(project.PhaseFilePath))
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.PhaseOrFileNotFound);
        var dto =await _fileService.DownloadFile(project.PhaseFilePath);
        if (dto==null)
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileDoesNotExist);
        dto.ContentType = _fileService.GetContentTypeFromPath(project.PhaseFilePath);
        return ServiceResponseFactory.Success(dto,Messages.PhaseFileDownloadedSuccessfully);
    }
    public async Task<ServiceResponse<GetPhaseForStudentDto>> GetPhaseByIdForStudentAsync(int phaseId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var phase = await _repository.GetPhaseByIdAsync(phaseId);
        
        if (phase == null || user==null || user.Student==null)
        {
            return ServiceResponseFactory.Failure<GetPhaseForStudentDto>(Messages.PhaseCantBeAccessed);
        }
        
        if (!await _studentClassRepository.IsStudentOfClassAsync(phase.Project.ClassId,user.Student.Id))
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

        if (project == null || user==null || user.Student==null || !await _studentClassRepository.IsStudentOfClassAsync(project.ClassId,user.Student.Id))
        {
            return ServiceResponseFactory.Failure<List<GetPhasesForStudentDto>>(Messages.ProjectCantBeAccessed);
        }
        var phases = await _repository.GetPhasesByProjectIdAsync(projectId);
        var dto =  _mapper.Map<List<GetPhasesForStudentDto>>(phases);
        return ServiceResponseFactory.Success(dto,Messages.PhasesRetrievedSuccessfully);
    }
    public async Task<ServiceResponse<FileDownloadDto>> HandleDownloadPhaseFileForStudentAsync(int phaseId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var phase = await _repository.GetPhaseByIdAsync(phaseId);
        if (phase == null || user==null || user.Student==null || !await _studentClassRepository.IsStudentOfClassAsync(phase.Project.ClassId,user.Student.Id))
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.PhaseCantBeAccessed);
        }
        var project = await _repository.GetPhaseByIdAsync(phaseId);
        if (project == null || string.IsNullOrWhiteSpace(project.PhaseFilePath))
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.PhaseOrFileNotFound);
        var dto =await _fileService.DownloadFile(project.PhaseFilePath);
        if (dto==null)
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileDoesNotExist);
        dto.ContentType = _fileService.GetContentTypeFromPath(project.PhaseFilePath);
        return ServiceResponseFactory.Success(dto,Messages.PhaseFileDownloadedSuccessfully);
    }

}