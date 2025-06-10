using AutoMapper;
using UCMS.DTOs;
using UCMS.DTOs.ExerciseDto;
using UCMS.Factories;
using UCMS.Models;
using UCMS.Repositories.ClassRepository.Abstraction;
using UCMS.Repositories.ExerciseRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.ExerciseService.Abstraction;
using UCMS.Services.FileService;

namespace UCMS.Services.ExerciseService;

public class ExerciseService:IExerciseService
{
    private readonly IExerciseRepository  _repository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IClassRepository _classRepository;
    private readonly IFileService _fileService;
    private readonly IStudentClassRepository _studentClassRepository;

    public ExerciseService(IExerciseRepository repository, IMapper mapper,IHttpContextAccessor httpContextAccessor,IClassRepository classRepository,IFileService fileService,IStudentClassRepository studentClassRepository)
    {
        _repository = repository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _classRepository = classRepository;
        _fileService = fileService;
        _studentClassRepository = studentClassRepository;
    }

    public async Task<ServiceResponse<GetExerciseForInstructorDto>> CreateExerciseAsync(int classId,CreateExerciseDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var currentClass = await _classRepository.GetClassByIdAsync(classId);
        if (currentClass == null)
        {
            return ServiceResponseFactory.Failure<GetExerciseForInstructorDto>(Messages.ClassNotFound);
        }
        if (currentClass.InstructorId != user!.Instructor!.Id)
        {
            return ServiceResponseFactory.Failure<GetExerciseForInstructorDto>(Messages.InvalidInstructorForThisClass);
        }
        var tehranZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");

        dto.StartDate = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Unspecified),
            tehranZone
        );

        dto.EndDate = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Unspecified),
            tehranZone
        );

        if (currentClass.StartDate.HasValue)
        {
            if (currentClass.StartDate.Value > DateOnly.FromDateTime(dto.StartDate.Date))
            {
                return ServiceResponseFactory.Failure<GetExerciseForInstructorDto>(Messages.ExerciseStartDateCannotBeBeforeClassStartDate);
            }
        }
        if (currentClass.EndDate.HasValue)
        {
            if (currentClass.EndDate.Value < DateOnly.FromDateTime(dto.EndDate.Date))
            {
                return ServiceResponseFactory.Failure<GetExerciseForInstructorDto>(Messages.ExerciseEndDateCannotBeAfterClassEndDate);
            }
        }
        var validator = new CreateExerciseDtoValidator(_fileService);
        var result = await validator.ValidateAsync(dto);
        var existingExercises = await _repository.GetExercisesByClassIdAsync(currentClass.Id);
        if (existingExercises.Any(p => p.Title.Trim().ToLower() == dto.Title.Trim().ToLower()))
        {
            return ServiceResponseFactory.Failure<GetExerciseForInstructorDto>(Messages.ExerciseAlreadyExists);
        }
        string? filePath = null;
        if (dto.ExerciseFile != null)
        {
            filePath = await _fileService.SaveFileAsync(dto.ExerciseFile, "exercises");
        }
        if (!result.IsValid)
        {
            var errorMessage = result.Errors.First().ErrorMessage;
            return ServiceResponseFactory.Failure<GetExerciseForInstructorDto>(errorMessage);
        }
        var newExercise = _mapper.Map<Exercise>(dto);
        newExercise.ClassId=currentClass.Id;
        newExercise.ExerciseFilePath = filePath;
        await _repository.AddAsync(newExercise);
        var phaseDto = _mapper.Map<GetExerciseForInstructorDto>(newExercise);
        return ServiceResponseFactory.Success(phaseDto, Messages.ExerciseCreatedSuccessfully);
    }

    public async Task<ServiceResponse<GetExerciseForInstructorDto>> GetExerciseByIdForInstructorAsync(int exerciseId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var exercise = await _repository.GetExerciseByIdAsync(exerciseId);
        
        if (exercise == null || exercise.Class.InstructorId != user?.Instructor?.Id)
        {
            return ServiceResponseFactory.Failure<GetExerciseForInstructorDto>(Messages.ExerciseCantBeAccessed);
        }

        var dto = _mapper.Map<GetExerciseForInstructorDto>(exercise);

        return ServiceResponseFactory.Success(dto, Messages.ExerciseRetrievedSuccessfully);
    }
    public async Task<ServiceResponse<GetExerciseForInstructorDto>> UpdateExerciseAsync(int exerciseId, PatchExerciseDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var existingExercise = await _repository.GetExerciseByIdAsync(exerciseId);
        if (existingExercise == null || existingExercise.Class.InstructorId !=  user?.Instructor?.Id)
            return ServiceResponseFactory.Failure<GetExerciseForInstructorDto>(Messages.ExerciseCantBeAccessed);
        var tehranZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");

        if (dto.StartDate.HasValue)
        {
            dto.StartDate = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(dto.StartDate.Value, DateTimeKind.Unspecified),
                tehranZone
            );
            if (existingExercise.Class.StartDate.HasValue)
            {
                if (existingExercise.Class.StartDate.Value > DateOnly.FromDateTime(dto.StartDate.Value.Date))
                {
                    return ServiceResponseFactory.Failure<GetExerciseForInstructorDto>(Messages.ExerciseStartDateCannotBeBeforeClassStartDate);
                }
            }
        }
        if (dto.EndDate.HasValue)
        {
            dto.EndDate = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Unspecified),
                tehranZone
            );
            if (existingExercise.Class.EndDate.HasValue)
            {
                if (existingExercise.Class.EndDate.Value < DateOnly.FromDateTime(dto.EndDate.Value.Date))
                {
                    return ServiceResponseFactory.Failure<GetExerciseForInstructorDto>(Messages.ExerciseEndDateCannotBeAfterClassEndDate);
                }
            }
        }
        var validator = new UpdateExerciseDtoValidator(_fileService);
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return ServiceResponseFactory.Failure<GetExerciseForInstructorDto>(validationResult.Errors.First().ErrorMessage);
        if (dto.Title != null)
        {
                    var isDuplicate = await _repository.ExistsWithTitleExceptIdAsync(dto.Title, existingExercise.ClassId, exerciseId);
                    if (isDuplicate)
                        return ServiceResponseFactory.Failure<GetExerciseForInstructorDto>(Messages.ExerciseAlreadyExists);
        }
        _mapper.Map(dto, existingExercise);

        if (dto.ExerciseFile != null)
        {
            if (existingExercise.ExerciseFilePath != null)
            {
                _fileService.DeleteFile(existingExercise.ExerciseFilePath);
            }
            existingExercise.ExerciseFilePath = await _fileService.SaveFileAsync(dto.ExerciseFile, "exercises");
        }

        existingExercise.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(existingExercise);

        var phaseDto = _mapper.Map<GetExerciseForInstructorDto>(existingExercise);
        return ServiceResponseFactory.Success(phaseDto, Messages.ExerciseUpdatedSuccessfully);
    }

    public async Task<ServiceResponse<string>>  DeleteExerciseAsync(int exerciseId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var exercise = await _repository.GetExerciseByIdAsync(exerciseId);
        if (exercise == null ||exercise.Class.InstructorId != user!.Instructor!.Id)
            return ServiceResponseFactory.Failure<string>(Messages.ExerciseCantBeAccessed);
        await _repository.DeleteAsync(exercise);
        if (exercise.ExerciseFilePath != null)
        {
                    _fileService.DeleteFile(exercise.ExerciseFilePath);
        }
        return ServiceResponseFactory.Success("Exercise deleted successfully", Messages.ExerciseDeletedSuccessfully);
    }
    public async Task<ServiceResponse<List<GetExercisesForInstructorDto>>> GetExercisesForInstructor(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var currentclass = await _classRepository.GetClassByIdAsync(classId);

        if (currentclass == null || currentclass.InstructorId != user?.Instructor?.Id)
        {
            return ServiceResponseFactory.Failure<List<GetExercisesForInstructorDto>>(Messages.ExerciseCantBeAccessed);
        }
        var phases = await _repository.GetExercisesByClassIdAsync(classId);
        var dto =  _mapper.Map<List<GetExercisesForInstructorDto>>(phases);
        return ServiceResponseFactory.Success(dto,Messages.ExercisesRetrievedSuccessfully);
    }
    public async Task<ServiceResponse<FileDownloadDto>> HandleDownloadExerciseFileForInstructorAsync(int exerciseId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var exercise = await _repository.GetExerciseByIdAsync(exerciseId);
        
        if (exercise == null || exercise.Class.InstructorId != user?.Instructor?.Id)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.ExerciseCantBeAccessed);
        }
        if (string.IsNullOrWhiteSpace(exercise.ExerciseFilePath))
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileNotFound);
        var dto =await _fileService.DownloadFile(exercise.ExerciseFilePath);
        if (dto==null)
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileDoesNotExist);
        dto.ContentType = GetContentTypeFromPath(exercise.ExerciseFilePath);
        return ServiceResponseFactory.Success(dto,Messages.ExerciseFileDownloadedSuccessfully);
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
    public async Task<ServiceResponse<GetExerciseForStudentDto>> GetExerciseByIdForStudentAsync(int exerciseId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var exercise = await _repository.GetExerciseByIdAsync(exerciseId);
        
        if (exercise == null)
        {
            return ServiceResponseFactory.Failure<GetExerciseForStudentDto>(Messages.ExerciseCantBeAccessed);
        }
        
        if (!await _studentClassRepository.IsStudentOfClassAsync(exercise.ClassId,user!.Student!.Id))
        {
            return ServiceResponseFactory.Failure<GetExerciseForStudentDto>(Messages.ExerciseCantBeAccessed);
        }

        var dto = _mapper.Map<GetExerciseForStudentDto>(exercise);

        return ServiceResponseFactory.Success(dto, Messages.ExerciseRetrievedSuccessfully);
    }
    public async Task<ServiceResponse<List<GetExercisesForStudentDto>>> GetExercisesForStudent(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var currentClass = await _classRepository.GetClassByIdAsync(classId);

        if (currentClass == null || !await _studentClassRepository.IsStudentOfClassAsync(classId,user!.Student!.Id))
        {
            return ServiceResponseFactory.Failure<List<GetExercisesForStudentDto>>(Messages.ExerciseCantBeAccessed);
        }
        var exercises = await _repository.GetExercisesByClassIdAsync(classId);
        var dto =  _mapper.Map<List<GetExercisesForStudentDto>>(exercises);
        return ServiceResponseFactory.Success(dto,Messages.ExercisesRetrievedSuccessfully);
    }
    public async Task<ServiceResponse<FileDownloadDto>> HandleDownloadExerciseFileForStudentAsync(int exerciseId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var exercise = await _repository.GetExerciseByIdAsync(exerciseId);
        if (exercise == null || !await _studentClassRepository.IsStudentOfClassAsync(exercise.ClassId,user!.Student!.Id))
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.ExerciseCantBeAccessed);
        }
        if (string.IsNullOrWhiteSpace(exercise.ExerciseFilePath))
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileNotFound);
        var dto =await _fileService.DownloadFile(exercise.ExerciseFilePath);
        if (dto==null)
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileDoesNotExist);
        dto.ContentType = GetContentTypeFromPath(exercise.ExerciseFilePath);
        return ServiceResponseFactory.Success(dto,Messages.ExerciseFileDownloadedSuccessfully);
    }
    public async Task<ServiceResponse<List<GetExercisesForInstructorDto>>> GetExercisesOfClassForInstructor()
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var exercises = await _repository.GetExercisesByInstructorIdAsync(user!.Instructor!.Id);
        var dto =  _mapper.Map<List<GetExercisesForInstructorDto>>(exercises);
        return ServiceResponseFactory.Success(dto,Messages.ExercisesRetrievedSuccessfully);
    }
    public async Task<ServiceResponse<List<GetExercisesForStudentDto>>> GetExercisesOfClassForStudent()
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var exercises = await _repository.GetExercisesByClassIdAsync(user!.Instructor!.Id);
        var dto =  _mapper.Map<List<GetExercisesForStudentDto>>(exercises);
        return ServiceResponseFactory.Success(dto,Messages.ExercisesRetrievedSuccessfully);
    }
}