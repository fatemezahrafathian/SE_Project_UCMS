using AutoMapper;
using UCMS.DTOs;
using UCMS.DTOs.ExerciseSubmissionDto;
using UCMS.Factories;
using UCMS.Models;
using UCMS.Repositories.ExerciseRepository.Abstraction;
using UCMS.Repositories.ExerciseSubmissionRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.ExerciseSubmissionService.Abstraction;
using UCMS.Services.FileService;

namespace UCMS.Services.ExerciseSubmissionService;

public class ExerciseSubmissionService: IExerciseSubmissionService
{
    private readonly IExerciseSubmissionRepository _exerciseSubmissionRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IFileService _fileService;
    private readonly IExerciseRepository _exerciseRepository;
    private readonly IMapper _mapper;
    
    public ExerciseSubmissionService(IExerciseSubmissionRepository exerciseSubmissionRepository, IHttpContextAccessor httpContextAccessor, IFileService fileService, IExerciseRepository exerciseRepository, IMapper mapper)
    {
        _exerciseSubmissionRepository = exerciseSubmissionRepository;
        _httpContextAccessor = httpContextAccessor;
        _fileService = fileService;
        _exerciseRepository = exerciseRepository;
        _mapper = mapper;
    }

    public async Task<ServiceResponse<GetExerciseSubmissionPreviewForStudentDto>> CreateExerciseSubmission(int exerciseId, CreateExerciseSubmissionDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var exercise = await _exerciseRepository.GetExerciseWithRelationsByIdAsync(exerciseId);
        if (exercise==null)
        {
            return ServiceResponseFactory.Failure<GetExerciseSubmissionPreviewForStudentDto>(Messages.ExerciseNotFound);
        }

        if (exercise.Class.ClassStudents.All(cs => cs.Student.Id != user!.Student!.Id))
        {
            return ServiceResponseFactory.Failure<GetExerciseSubmissionPreviewForStudentDto>(Messages.CanNotaccessExercise);
        }

        var validator = new CreateExerciseSubmissionDtoValidator();
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errorMessage = result.Errors.First().ErrorMessage;
            return ServiceResponseFactory.Failure<GetExerciseSubmissionPreviewForStudentDto>(errorMessage);
        }

        if (!_fileService.IsValidExtension(dto.SubmissionFile!, exercise.FileFormats))
        {
            return ServiceResponseFactory.Failure<GetExerciseSubmissionPreviewForStudentDto>(Messages.InvalidFormat);
        }

        if (!_fileService.IsValidFileSize(dto.SubmissionFile!))
        {
            return ServiceResponseFactory.Failure<GetExerciseSubmissionPreviewForStudentDto>(Messages.InvalidSize);
        }
        
        var filePath = await _fileService.SaveFileAsync(dto.SubmissionFile!, "exercise-submissions");

        var currentFinalExerciseSubmission =
            await _exerciseSubmissionRepository.GetFinalExerciseSubmissionsAsync(
                user!.Student!.Id, exerciseId);
        
        if (currentFinalExerciseSubmission != null)
        {
            currentFinalExerciseSubmission.IsFinal = false;
            await _exerciseSubmissionRepository.UpdateExerciseSubmissionAsync(currentFinalExerciseSubmission);
        }

        var newExerciseSubmission = new ExerciseSubmission()
        {
            ExerciseId = exerciseId,
            StudentId = user.Student!.Id,
            FilePath = filePath
        };
        
        await _exerciseSubmissionRepository.AddExerciseSubmissionAsync(newExerciseSubmission);

        var newExerciseSubmissionDto = _mapper.Map<GetExerciseSubmissionPreviewForStudentDto>(newExerciseSubmission);
        newExerciseSubmissionDto.FileType = Path.GetExtension((string?) filePath)?.TrimStart('.').ToLower() ?? "unknown";

        return ServiceResponseFactory.Success(newExerciseSubmissionDto, Messages.ExerciseSubmissionCreatedSuccessfully);
        
    }

    public async Task<ServiceResponse<FileDownloadDto>> GetExerciseSubmissionFileForInstructor(int exerciseSubmissionId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var exerciseSubmission = await _exerciseSubmissionRepository.GetExerciseSubmissionForInstructorByIdAsync(exerciseSubmissionId);
        if (exerciseSubmission==null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.ExerciseSubmissionNotFound);
        }
        
        if (exerciseSubmission.Exercise.Class.InstructorId!=user!.Instructor!.Id || !exerciseSubmission.IsFinal)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.ExerciseSubmissionCanNotBeAccessed);
        }

        var dto = await _fileService.DownloadFile2(exerciseSubmission.FilePath);
        if (dto == null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileDoesNotExist);
        }

        return ServiceResponseFactory.Success(dto, Messages.ExerciseSubmissionFileFetchedSuccessfully);
        
    }

    public async Task<ServiceResponse<FileDownloadDto>> GetExerciseSubmissionFileForStudent(int exerciseSubmissionId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var exerciseSubmission = await _exerciseSubmissionRepository.GetExerciseSubmissionByIdAsync(exerciseSubmissionId);
        if (exerciseSubmission==null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.ExerciseSubmissionNotFound);
        }

        if (exerciseSubmission.StudentId != user!.Student!.Id)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.ExerciseSubmissionCanNotBeAccessed);
        }

        var dto = await _fileService.DownloadFile2(exerciseSubmission.FilePath);
        if (dto == null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileDoesNotExist);
        }

        return ServiceResponseFactory.Success(dto, Messages.ExerciseSubmissionFileFetchedSuccessfully);
        
    }

    public async Task<ServiceResponse<FileDownloadDto>> GetExerciseSubmissionFiles(int exerciseId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var exercise = await _exerciseRepository.GetExerciseByIdAsync(exerciseId);
        if (exercise==null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.ExerciseNotFound);
        }
        
        if (exercise.Class.InstructorId!=user!.Instructor!.Id)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.CanNotaccessExercise);
        }
        
        var submissions = await _exerciseSubmissionRepository.GetExerciseSubmissionsAsync(exerciseId);
        if (submissions.Count == 0)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.NoExerciseSubmissionFound);
        }
        
        var filePaths = submissions
            .Select(s => s.FilePath)
            .ToList();
        
        var zipFile = await _fileService.ZipFiles(filePaths);
        if (zipFile==null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileDoesNotExist);
        }

        return ServiceResponseFactory.Success(zipFile, Messages.ExerciseSubmissionFilesFetchedSuccessfully);
        
    }

    public async Task<ServiceResponse<List<GetExerciseSubmissionPreviewForInstructorDto>>> GetExerciseSubmissionsForInstructor(SortExerciseSubmissionsForInstructorDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var exercise = await _exerciseRepository.GetExerciseByIdAsync(dto.ExerciseId);
        if (exercise==null)
        {
            return ServiceResponseFactory.Failure<List<GetExerciseSubmissionPreviewForInstructorDto>>(Messages.ExerciseNotFound);
        }
        
        if (exercise.Class.InstructorId!=user!.Instructor!.Id)
        {
            return ServiceResponseFactory.Failure<List<GetExerciseSubmissionPreviewForInstructorDto>>(Messages.CanNotaccessExercise);
        }
        
        var validator = new SortExerciseSubmissionForInstructorDtoValidator();
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errorMessage = result.Errors.First().ErrorMessage;
            return ServiceResponseFactory.Failure<List<GetExerciseSubmissionPreviewForInstructorDto>>(errorMessage);
        }

        var submissions = await _exerciseSubmissionRepository.GetExerciseSubmissionsForInstructorByExerciseIdAsync(dto.ExerciseId, dto.SortBy, dto.SortOrder);

        var submissionDtos = _mapper.Map<List<GetExerciseSubmissionPreviewForInstructorDto>>(submissions);
        
        var submissionDict = submissions.ToDictionary(s => s.Id, s => s.FilePath);

        foreach (var dtoItem in submissionDtos)
        {
            if (submissionDict.TryGetValue(dtoItem.Id, out var filePath))
            {
                dtoItem.FileType = Path.GetExtension((string?) filePath)?.TrimStart('.').ToLower() ?? "unknown";
            }
        }
        
        return ServiceResponseFactory.Success(submissionDtos, Messages.ExerciseSubmissionsFetchedSuccessfully);

    }

    public async Task<ServiceResponse<List<GetExerciseSubmissionPreviewForStudentDto>>> GetExerciseSubmissionsForStudent(SortExerciseSubmissionsStudentDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var exercise = await _exerciseRepository.GetExerciseWithRelationsByIdAsync(dto.ExerciseId);
        if (exercise==null)
        {
            return ServiceResponseFactory.Failure<List<GetExerciseSubmissionPreviewForStudentDto>>(Messages.ExerciseNotFound);
        }

        if (exercise.Class.ClassStudents.All(cs => cs.Student.Id != user!.Student!.Id))
        {
            return ServiceResponseFactory.Failure<List<GetExerciseSubmissionPreviewForStudentDto>>(Messages.CanNotaccessExercise);
        }
        
        var validator = new SortExerciseSubmissionForStudentDtoValidator();
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errorMessage = result.Errors.First().ErrorMessage;
            return ServiceResponseFactory.Failure<List<GetExerciseSubmissionPreviewForStudentDto>>(errorMessage);
        }

        var submissions = await _exerciseSubmissionRepository.GetExerciseSubmissionsForStudentByExerciseIdAsync(user!.Student!.Id, dto.ExerciseId, dto.SortBy, dto.SortOrder);

        var submissionDtos = _mapper.Map<List<GetExerciseSubmissionPreviewForStudentDto>>(submissions);
        
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

    public Task<ServiceResponse<FileDownloadDto>> GetExerciseScoreTemplateFile(int exerciseId)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResponse<string>> UpdateFinalExerciseSubmission(int exerciseSubmissionId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var exerciseSubmission = await _exerciseSubmissionRepository.GetExerciseSubmissionByIdAsync(exerciseSubmissionId);
        if (exerciseSubmission==null)
        {
            return ServiceResponseFactory.Failure<string>(Messages.ExerciseSubmissionNotFound);
        }
        
        if (exerciseSubmission.StudentId!=user!.Student!.Id)
        {
            return ServiceResponseFactory.Failure<string>(Messages.ExerciseSubmissionCanNotBeAccessed);
        }
        
        if (exerciseSubmission.IsFinal)
        {
            return ServiceResponseFactory.Failure<string>(Messages.ExerciseSubmissionMarkedAsFinalAlready);
        }
        
        var currentFinalExerciseSubmission =
            await _exerciseSubmissionRepository.GetFinalExerciseSubmissionsAsync(
                user!.Student!.Id, exerciseSubmission.ExerciseId);
        
        if (currentFinalExerciseSubmission != null)
        {
            currentFinalExerciseSubmission.IsFinal = false;
            await _exerciseSubmissionRepository.UpdateExerciseSubmissionAsync(currentFinalExerciseSubmission);
        }

        exerciseSubmission.IsFinal = true;
        await _exerciseSubmissionRepository.UpdateExerciseSubmissionAsync(exerciseSubmission);

        return ServiceResponseFactory.Success<string>(Messages.ExerciseSubmissionMarkedAsFinalSuccessfully);

    }

    public Task<ServiceResponse<string>> UpdateExerciseSubmissionScore(int exerciseSubmissionId, UpdateExerciseSubmissionScoreDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResponse<string>> UpdateExerciseSubmissionScores(int exerciseId, IFormFile scoreFile)
    {
        throw new NotImplementedException();
    }
}