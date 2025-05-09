using AutoMapper;
using UCMS.DTOs;
using UCMS.DTOs.ClassDto;
using UCMS.Extensions;
using UCMS.Factories;
using UCMS.Models;
using UCMS.Repositories.ClassRepository.Abstraction;
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

    public ClassService(IClassRepository classRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor, IImageService imageService, IPasswordService passwordService, IStudentClassService studentClassService)
    {
        _classRepository = classRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _imageService = imageService;
        _passwordService = passwordService;
        _studentClassService = studentClassService;
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
    
}