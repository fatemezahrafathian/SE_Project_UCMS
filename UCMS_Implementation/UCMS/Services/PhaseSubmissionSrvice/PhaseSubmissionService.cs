using AutoMapper;
using UCMS.DTOs;
using UCMS.DTOs.TeamPhaseDto;
using UCMS.Factories;
using UCMS.Models;
using UCMS.Repositories.PhaseRepository.Abstraction;
using UCMS.Repositories.PhaseSubmissionRepository;
using UCMS.Repositories.StudentTeamPhaseRepository.Abstraction;
using UCMS.Repositories.TeamPhaseRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.FileService;

namespace UCMS.Services.TeamPhaseSrvice;

public class PhaseSubmissionService: IPhaseSubmissionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPhaseSubmissionRepository _phaseSubmissionRepository;
    private readonly IFileService _fileService;
    private readonly IMapper _mapper;
    private readonly IPhaseRepository _phaseRepository;
    private readonly IStudentTeamPhaseRepository _studentTeamPhaseRepository;
    
    public PhaseSubmissionService(IHttpContextAccessor httpContextAccessor, IPhaseSubmissionRepository phaseSubmissionRepository, IFileService fileService, IMapper mapper, IPhaseRepository phaseRepository, IStudentTeamPhaseRepository studentTeamPhaseRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _phaseSubmissionRepository = phaseSubmissionRepository;
        _fileService = fileService;
        _mapper = mapper;
        _phaseRepository = phaseRepository;
        _studentTeamPhaseRepository = studentTeamPhaseRepository;
    }
    
    // make the last final
    public async Task<ServiceResponse<string>> CreatePhaseSubmission(int phaseId, CreatePhaseSubmissionDto dto) // return submission dto
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var phase = await _phaseRepository.GetPhaseSimpleByIdAsync(phaseId);
        if (phase==null)
        {
            return ServiceResponseFactory.Failure<string>(Messages.PhaseNotFound);
        }

        var studentTeamPhase = await _studentTeamPhaseRepository.GetStudentTeamPhaseAsync(user!.Student!.Id, phaseId);
        if (studentTeamPhase==null)
        {
            return ServiceResponseFactory.Failure<string>(Messages.StudentInNoTeamForThisPhase);
        }
        
        var validator = new CreatePhaseSubmissionDtoValidator();
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errorMessage = result.Errors.First().ErrorMessage;
            return ServiceResponseFactory.Failure<string>(errorMessage);
        }

        _fileService.IsValidExtension(dto.SubmissionFile!, phase.FileFormats);  // make it not nullable
        _fileService.IsValidFileSize(dto.SubmissionFile!);
        var filePath = await _fileService.SaveFileAsync(dto.SubmissionFile!, "phase-submissions");

        var newPhaseSubmission = _mapper.Map<PhaseSubmission>(dto);
        newPhaseSubmission.StudentTeamPhaseId = studentTeamPhase.Id;
        newPhaseSubmission.FilePath = filePath;
        newPhaseSubmission.IsFinal = true;
            
        await _phaseSubmissionRepository.AddPhaseSubmissionAsync(newPhaseSubmission);
        
        return ServiceResponseFactory.Success<string>(Messages.PhaseSubmissionCreatesSuccessfully);
    }

    public async Task<ServiceResponse<FileDownloadDto>> GetPhaseSubmissionFileForInstructor(int submissionId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var submission = await _phaseSubmissionRepository.GetPhaseSubmissionForInstructorByIdAsync(submissionId);
        if (submission==null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.PhaseSubmissionNotFound);
        }
        
        if (submission.StudentTeamPhase.Phase.Project.Class.InstructorId!=user!.Instructor!.Id)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.PhaseSubmissionCanNotBeAccessed);
        }

        var dto = await _fileService.DownloadFile(submission.FilePath);
        if (dto == null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileDoesNotExist);
        }

        return ServiceResponseFactory.Success(dto, Messages.PhaseSubmissionFileFetchedSuccessfully);
    }
    
    public async Task<ServiceResponse<FileDownloadDto>> GetPhaseSubmissionFileForStudent(int submissionId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var submission = await _phaseSubmissionRepository.GetPhaseSubmissionForStudentByIdAsync(submissionId);
        if (submission == null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.PhaseSubmissionNotFound);
        }
        
        if (!await _studentTeamPhaseRepository.AnyStudentTeamPhaseAsync(user!.Student!.Id, submission.StudentTeamPhase.StudentTeam.TeamId, submission.StudentTeamPhase.PhaseId))
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.PhaseSubmissionCanNotBeAccessed);
        }

        var dto = await _fileService.DownloadFile(submission.FilePath);
        if (dto == null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileDoesNotExist);
        }

        return ServiceResponseFactory.Success(dto, Messages.PhaseSubmissionFileFetchedSuccessfully);
    }


    public async Task<ServiceResponse<List<FileDownloadDto>>> GetPhaseSubmissionFiles(int phaseId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var phase = await _phaseRepository.GetPhaseByIdAsync(phaseId);
        if (phase == null || phase.Project.Class.InstructorId != user?.Instructor?.Id)
        {
            return ServiceResponseFactory.Failure<List<FileDownloadDto>>(Messages.PhaseCantBeAccessed);
        }

        var submissions = await _phaseSubmissionRepository.GetPhaseSubmissionsAsync(phaseId);

        var filePaths = submissions
            .Select(s => s.FilePath)
            .ToList();

        var downloadDtos = await _fileService.DownloadFiles(filePaths);

        var validDownloads = downloadDtos.Where(f => f != null).Cast<FileDownloadDto>().ToList();

        return ServiceResponseFactory.Success(validDownloads, Messages.PhaseSubmissionFilesFetchedSuccessfully);
    }

    public async Task<ServiceResponse<List<GetSubmissionPreviewForInstructorDto>>> GetPhaseSubmissionsForInstructor(SortPhaseSubmissionsForInsrtuctorDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var phase = await _phaseRepository.GetPhaseByIdAsync(dto.PhaseId);
        if (phase == null || phase.Project.Class.InstructorId != user?.Instructor?.Id)
        {
            return ServiceResponseFactory.Failure<List<GetSubmissionPreviewForInstructorDto>>(Messages.PhaseCantBeAccessed);
        }

        var submissions = await _phaseSubmissionRepository.GetPhaseSubmissionsForInstructorByPhaseIdAsync(dto.PhaseId, dto.SortBy, dto.SortOrder);

        var submissionDtos = _mapper.Map<List<GetSubmissionPreviewForInstructorDto>>(submissions);
        
        var submissionDict = submissions.ToDictionary(s => s.Id, s => s.FilePath);

        foreach (var dtoItem in submissionDtos)
        {
            if (submissionDict.TryGetValue(dtoItem.Id, out var filePath))
            {
                dtoItem.FileType = Path.GetExtension((string?) filePath)?.TrimStart('.').ToLower() ?? "unknown";
            }
        }
        
        return ServiceResponseFactory.Success(submissionDtos, Messages.PhaseSubmissionsFetchedSuccessfully);
    }

    public async Task<ServiceResponse<List<GetSubmissionPreviewForStudentDto>>> GetPhaseSubmissionsForStudent(SortPhaseSubmissionsStudentDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var studentTeamPhase = await _studentTeamPhaseRepository.GetStudentTeamPhaseWithRelationAsync(user!.Student!.Id, dto.PhaseId);
        if (studentTeamPhase==null)
        {
            return ServiceResponseFactory.Failure<List<GetSubmissionPreviewForStudentDto>>(Messages.StudentInNoTeamForThisPhase);
        }

        var submissions = await _phaseSubmissionRepository.GetPhaseSubmissionsForStudentByPhaseIdAsync(studentTeamPhase.StudentTeam.TeamId, dto.PhaseId, dto.SortBy, dto.SortOrder);

        var submissionDtos = _mapper.Map<List<GetSubmissionPreviewForStudentDto>>(submissions);
        
        var submissionDict = submissions.ToDictionary(s => s.Id, s => s.FilePath);

        foreach (var dtoItem in submissionDtos)
        {
            if (submissionDict.TryGetValue(dtoItem.Id, out var filePath))
            {
                dtoItem.FileType = Path.GetExtension((string?) filePath)?.TrimStart('.').ToLower() ?? "unknown";
            }
        }
        
        return ServiceResponseFactory.Success(submissionDtos, Messages.PhaseSubmissionsFetchedSuccessfully);
    }

    public async Task<ServiceResponse<string>> UpdateFinalSubmission(int submissionId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var submission = await _phaseSubmissionRepository.GetPhaseSubmissionForStudentByIdAsync(submissionId);
        if (submission == null)
        {
            return ServiceResponseFactory.Failure<string>(Messages.PhaseSubmissionNotFound);
        }

        var studentTeamPhase = await _studentTeamPhaseRepository.GetStudentTeamPhaseWithRelationAsync(user!.Student!.Id, submission.StudentTeamPhase.StudentTeam.TeamId, submission.StudentTeamPhase.PhaseId);
        if (studentTeamPhase==null)
        {
            return ServiceResponseFactory.Failure<string>(Messages.PhaseSubmissionCanNotBeAccessed);
        }

        if (submission.IsFinal)
        {
            return ServiceResponseFactory.Success<string>(Messages.PhaseSubmissionMarkedAsFinalAlready);
        }
        
        var currentFinalPhaseSubmission =
            await _phaseSubmissionRepository.GetFinalPhaseSubmissionsAsync(studentTeamPhase.PhaseId,
                studentTeamPhase.StudentTeam.TeamId);
        
        if (currentFinalPhaseSubmission != null)
        {
            currentFinalPhaseSubmission.IsFinal = false;
            await _phaseSubmissionRepository.UpdatePhaseSubmissionAsync(currentFinalPhaseSubmission);
        }

        submission.IsFinal = true;
        await _phaseSubmissionRepository.UpdatePhaseSubmissionAsync(submission);

        return ServiceResponseFactory.Success<string>(Messages.PhaseSubmissionMarkedAsFinal);
    }
}