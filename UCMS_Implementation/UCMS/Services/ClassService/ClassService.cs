using AutoMapper;
using UCMS.DTOs;
using UCMS.DTOs.ClassDto;
using UCMS.Extensions;
using UCMS.Factories;
using UCMS.Models;
using UCMS.Repositories.ClassRepository.Abstraction;
using UCMS.Repositories.ExamRepository.Abstraction;
using UCMS.Repositories.ExerciseRepository.Abstraction;
using UCMS.Repositories.PhaseRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.ClassService.Abstraction;
using UCMS.Services.ImageService;
using UCMS.Services.PasswordService.Abstraction;

namespace UCMS.Services.ClassService;

public class ClassService: IClassService
{
    private readonly IClassRepository _classRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IImageService _imageService;
    private readonly IPasswordService _passwordService;
    private readonly IStudentClassService _studentClassService;
    private readonly IPhaseRepository _phaseRepository;
    private readonly IExerciseRepository _exerciseRepository;
    private readonly IExamRepository _examRepository;

    public ClassService(IClassRepository classRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor, IImageService imageService, IPasswordService passwordService, IStudentClassService studentClassService, IPhaseRepository phaseRepository, IExerciseRepository exerciseRepository, IExamRepository examRepository)
    {
        _classRepository = classRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _imageService = imageService;
        _passwordService = passwordService;
        _studentClassService = studentClassService;
        _phaseRepository = phaseRepository;
        _exerciseRepository = exerciseRepository;
        _examRepository = examRepository;
    }

    public async Task<ServiceResponse<GetClassForInstructorDto>> CreateClass(CreateClassDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var newClass = _mapper.Map<Class>(dto);
        newClass.InstructorId = user!.Instructor!.Id;
        
        var validator = new CreateClassDtoValidator(_imageService);
        var result = await validator.ValidateAsync(dto);

        if (!result.IsValid)
        {
            var errorMessage = result.Errors.First().ErrorMessage;
            return ServiceResponseFactory.Failure<GetClassForInstructorDto>(errorMessage);
        }
        if (dto.ProfileImage != null)
        {
            
            var imageUrl = await _imageService.SaveImageAsync(dto.ProfileImage, "images/classes"); // get from appsetting
            newClass.ProfileImageUrl = imageUrl; 
        }
        
        newClass.ClassCode = await GenerateUniqueClassCodeAsync();
        
        newClass.PasswordSalt = _passwordService.CreateSalt();
        newClass.PasswordHash = await _passwordService.HashPasswordAsync(dto.Password, newClass.PasswordSalt);

        await _classRepository.AddClassAsync(newClass);

        var responseDto = _mapper.Map<GetClassForInstructorDto>(newClass);

        return ServiceResponseFactory.Success(responseDto, Messages.ClassCreatedSuccessfully);
    }
    
    public async Task<ServiceResponse<GetClassForInstructorDto>> GetClassForInstructor(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var classEntity = await _classRepository.GetInstructorClassByClassIdAsync(classId);
        if (classEntity == null)
        {
            return ServiceResponseFactory.Failure<GetClassForInstructorDto>(
                Messages.ClassNotFound);
        }
        
        var isInstructorOfClass = classEntity.InstructorId == user!.Instructor!.Id;
        if (!isInstructorOfClass)
        {
            return ServiceResponseFactory.Failure<GetClassForInstructorDto>(Messages.ClassCan_tBeAccessed); // edit this
        }
        
        var responseDto = _mapper.Map<GetClassForInstructorDto>(classEntity);
        // responseDto.StudentCount = await _studentClassService.GetStudentClassCount(classId);
        return ServiceResponseFactory.Success(responseDto, Messages.ClassFetchedSuccessfully);
    }
    
    private async Task<string> GenerateUniqueClassCodeAsync()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        string code;

        do
        {
            code = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        while (await _classRepository.ClassCodeExistsAsync(code));

        return code;
    }
    
    public async Task<ServiceResponse<GetClassPageDto>> GetClassesForInstructor(PaginatedFilterClassForInstructorDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var classEntityQueryable = _classRepository.FilterInstructorClassesByInstructorIdAsync(user!.Instructor!.Id, dto.Title, dto.IsActive);        
        var classEntityList = await classEntityQueryable.PaginateAsync(dto.Page, dto.PageSize);

        var responseDto = _mapper.Map<GetClassPageDto>(classEntityList);
        
        return ServiceResponseFactory.Success(responseDto, Messages.ClassesRetrievedSuccessfully); // ClassesFetchedSuccessfully
    }

    public async Task<ServiceResponse<List<GetClassEntryDto>>> GetClassEntries(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var cls = await _classRepository.GetClassWithEntriesAsync(classId);
        if (cls == null)
        {
            return ServiceResponseFactory.Failure<List<GetClassEntryDto>>(Messages.ClassNotFound);
        }
        if (cls.InstructorId != user!.Instructor!.Id)
        {
            return ServiceResponseFactory.Failure<List<GetClassEntryDto>>(Messages.ClassCantBeAccessed);
        }

        var entriesList = (from project in cls.Projects
        from phase in project.Phases
        select new GetClassEntryDto()
        {
            EntryId = phase.Id,
            EntryType = EntryType.Phase,
            EntryName = phase.Title,
            PartialScore = phase.PhaseScore,
            PortionInTotalScore = phase.PortionInTotalScore
        }).ToList();
        
        entriesList.AddRange(cls.Exercises.Select(exercise => new GetClassEntryDto()
        {
            EntryId = exercise.Id,
            EntryType = EntryType.Exercise,
            EntryName = exercise.Title,
            PartialScore = exercise.ExerciseScore,
            PortionInTotalScore = exercise.PortionInTotalScore
        }));

        entriesList.AddRange(cls.Exams.Select(exam => new GetClassEntryDto()
        {
            EntryId = exam.Id,
            EntryType = EntryType.Exam,
            EntryName = exam.Title,
            PartialScore = exam.ExamScore,
            PortionInTotalScore = exam.PortionInTotalScore
        }));

        return ServiceResponseFactory.Success(entriesList, Messages.ClassEntriesFetchedSuccessfully);

    }

    public async Task<ServiceResponse<string>> DeleteClass(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var classEntity = await _classRepository.GetClassByIdAsync(classId);
        if (classEntity == null)
        {
            return ServiceResponseFactory.Failure<string>(
                Messages.ClassNotFound);
        }

        var isInstructor = classEntity.InstructorId == user!.Instructor!.Id;
        if (!isInstructor)
        {
            return ServiceResponseFactory.Failure<string>(
                Messages.ClassCan_tBeAccessed); // classCanNotBeAccessed
        }

        await _classRepository.DeleteClassAsync(classEntity);
        
        return ServiceResponseFactory.Success<string>(Messages.ClassDeletedSuccessfully);
    }
    public async Task<ServiceResponse<GetClassForInstructorDto>> UpdateClassPartial(int classId, PatchClassDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var classEntity = await _classRepository.GetInstructorClassByClassIdAsync(classId); // change the name of repository functions
        if (classEntity == null)
        {
            return ServiceResponseFactory.Failure<GetClassForInstructorDto>(
                Messages.ClassNotFound);
        }
        
        var isInstructor = classEntity.InstructorId == user!.Instructor!.Id;
        if (!isInstructor)
        {
            return ServiceResponseFactory.Failure<GetClassForInstructorDto>(
                Messages.ClassCan_tBeAccessed);
        }
        
        var validator = new UpdateClassDtoValidator(_imageService);
        validator.ApplyRuntimeRules(classEntity.CreatedAt, classEntity.UpdatedAt, classEntity.StartDate, classEntity.EndDate);

        var result = await validator.ValidateAsync(dto);

        if (!result.IsValid)
        {
            var errorMessage = string.Join(" | ", result.Errors.Select(e => e.ErrorMessage));
            return ServiceResponseFactory.Failure<GetClassForInstructorDto>(errorMessage);
        }

        if (dto.ProfileImage != null)
        {
            _imageService.DeleteImage(classEntity.ProfileImageUrl);
          var imageUrl = await _imageService.SaveImageAsync(dto.ProfileImage, "images/classes"); // get from appsetting
          classEntity.ProfileImageUrl = imageUrl;  
        }
        

        _mapper.Map(dto, classEntity);
            
            
        classEntity.UpdatedAt = DateTime.UtcNow;

        await _classRepository.UpdateClassAsync(classEntity);
        
        var responseDto = _mapper.Map<GetClassForInstructorDto>(classEntity);
        return ServiceResponseFactory.Success(responseDto, Messages.ClassUpdatedSuccessfully);
    }

    public async Task<ServiceResponse<string>> UpdateClassEntries(int classId, UpdateClassEntriesDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var cls = await _classRepository.GetClassWithEntriesAsync(classId);
        if (cls == null)
        {
            return ServiceResponseFactory.Failure<string>(Messages.ClassNotFound);
        }
        if (cls.InstructorId != user!.Instructor!.Id)
        {
            return ServiceResponseFactory.Failure<string>(Messages.ClassCantBeAccessed);
        }

        var validator = new UpdateClassEntriesDtoDtoValidator();
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errorMessage = result.Errors.First().ErrorMessage;
            return ServiceResponseFactory.Failure<string>(errorMessage);
        }

        var entriesList = new List<(int Id, EntryType Type)>();
        entriesList.AddRange(
            cls.Projects.SelectMany(p => p.Phases)
                .Select(phase => (phase.Id, EntryType.Phase)));
        entriesList.AddRange(
            cls.Exercises.Select(ex => (ex.Id, EntryType.Exercise)));
        entriesList.AddRange(
            cls.Exams.Select(exam => (exam.Id, EntryType.Exam)));
        var dtoEntriesList = dto.EntryDtos
            .Select(e => (e.EntryId, e.EntryType))
            .ToList();
        if (entriesList.Count != dtoEntriesList.Count)
        {
            return ServiceResponseFactory.Failure<string>(Messages.EntriesMismatchWithClassEntries);
        }

        var setA = entriesList.ToHashSet();
        var setB = dtoEntriesList.ToHashSet();
        if (!setA.SetEquals(setB))
        {
            return ServiceResponseFactory.Failure<string>(Messages.EntriesMismatchWithClassEntries);
        }
        
        foreach (var entry in dto.EntryDtos)
        {
            switch (entry.EntryType)
            {
                case EntryType.Phase:

                    var phase = await _phaseRepository.GetPhaseByIdAsync(entry.EntryId);
                    if (phase == null)
                    {
                        return ServiceResponseFactory.Failure<string>(Messages.PhaseNotFound);
                    }
                    if (phase.Project.ClassId!=classId)
                    {
                        return ServiceResponseFactory.Failure<string>(Messages.PhaseCantBeAccessed);
                    }
                    phase.PortionInTotalScore = entry.PortionInTotalScore;
                    await _phaseRepository.UpdateAsync(phase);
                    break;
                
                case EntryType.Exercise:
                    
                    var exercise = await _exerciseRepository.GetExerciseByIdAsync(entry.EntryId);
                    if (exercise == null)
                    {
                        return ServiceResponseFactory.Failure<string>(Messages.ExerciseNotFound);
                    }
                    if (exercise.ClassId!=classId)
                    {
                        return ServiceResponseFactory.Failure<string>(Messages.ExerciseCantBeAccessed);
                    }
                    exercise.PortionInTotalScore = entry.PortionInTotalScore;
                    await _exerciseRepository.UpdateAsync(exercise);
                    break;

                case EntryType.Exam:
                    
                    var exam = await _examRepository.GetExamByIdAsync(entry.EntryId);
                    if (exam == null)
                    {
                        return ServiceResponseFactory.Failure<string>(Messages.ExamNotFound);
                    }
                    if (exam.ClassId!=classId)
                    {
                        return ServiceResponseFactory.Failure<string>(Messages.ExamCantBeAccessed);
                    }
                    exam.PortionInTotalScore = entry.PortionInTotalScore;
                    await _examRepository.UpdateAsync(exam);
                    break;
                default:
                    return ServiceResponseFactory.Failure<string>(Messages.InvalidEntryType);
            }
        }

        cls.TotalScore = dto.TotalScore;
        await _classRepository.UpdateClassAsync(cls);
        
        return ServiceResponseFactory.Success<string>(Messages.ClassEntriesUpdatedSuccessfully);

    }
}