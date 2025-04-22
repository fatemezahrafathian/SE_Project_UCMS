using AutoMapper;
using UCMS.DTOs;
using UCMS.DTOs.ClassDto;
using UCMS.Models;
using UCMS.Repositories;
using UCMS.Repositories.ClassRepository.Abstraction;
using UCMS.Repositories.InstructorRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.ClassService.Abstraction;

namespace UCMS.Services.ClassService;

public class ClassService: IClassService
{
    private readonly IClassRepository _classRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IInstructorRepository _instructorRepository;
    

    public ClassService(IClassRepository classRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor, IInstructorRepository instructorRepository)
    {
        _classRepository = classRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _instructorRepository = instructorRepository;
    }

    public async Task<ServiceResponse<GetClassDto>> CreateClass(CreateClassDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var instructor = await _instructorRepository.GetInstructorByUserIdAsync(user.Id);  // add this part to middleware

        var newClass = _mapper.Map<Class>(dto);
        newClass.InstructorId = instructor.Id;

        newClass.ClassCode = await GenerateUniqueClassCodeAsync();

        await _classRepository.AddClassAsync(newClass);

        var responseDto = _mapper.Map<GetClassDto>(newClass);
        responseDto.InstructorFullName = $"{user.FirstName} {user.LastName}";
        
        return new ServiceResponse<GetClassDto>
        {
            Data = responseDto,
            Message = Messages.ClassCreatedSuccessfully,
            Success = true
        };
    }

    public async Task<ServiceResponse<GetClassDto>> GetClassById(int classId) // student and instructor who are part of the class
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var classEntity = await _classRepository.GetClassByIdAsync(classId);
        if (classEntity == null)
        {
            return new ServiceResponse<GetClassDto>
            {
                Success = false,
                Message = Messages.ClassNotFound
            };
        }
        
        var isInstructor = classEntity.Instructor.UserId == user.Id;
        // var student = _studentRepository.GetStudentByUserId(user.Id);
        // var studentClasses = _studentClassRepository.GetClassesOfStudent(student.Id);
        // var isStudent = studentClasses.Any(sc => sc.ClassId == classId);
        if (!isInstructor ) // && !isStudent
        {
            return new ServiceResponse<GetClassDto> 
            {
                Success = false,
                Message = Messages.ClassCan_tBeAccessed
            };
        }
        
        // second mapper
        var responseDto = _mapper.Map<GetClassDto>(classEntity);

        return new ServiceResponse<GetClassDto>
        {
            Data = responseDto,
            // Data = null,
            Message = Messages.ClassFetchedSuccessfully,
            Success = true
        };
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
        while (await _classRepository.IsClassCodeExistAsync(code));

        return code;
    }
    
    public async Task<ServiceResponse<List<GetClassPreviewDto>>> GetClassesByInstructor()
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var instructor = await _instructorRepository.GetInstructorByUserIdAsync(user.Id);
        
        var classList = await _classRepository.GetClassesByInstructorAsync(instructor.Id);
        
        var clsDto = _mapper.Map<List<GetClassPreviewDto>>(classList);

        return new ServiceResponse<List<GetClassPreviewDto>> // add constructor for null data responses
        {
            Data = clsDto,
            Success = true,
            Message = Messages.ClassesRetrievedSuccessfully
        };
    }

    public async Task<ServiceResponse<string>> DeleteClass(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var classEntity = await _classRepository.GetClassByIdAsync(classId);
        if (classEntity == null)
        {
            return new ServiceResponse<string>
            {
                Success = false,
                Message = Messages.ClassNotFound
            };
        }

        var isInstructor = classEntity.Instructor.UserId == user.Id;
        if (!isInstructor)
        {
            return new ServiceResponse<string> 
            {
                Success = false,
                Message = Messages.ClassCan_tBeAccessed
            };
        }

        await _classRepository.DeleteClassAsync(classEntity);
        
        return new ServiceResponse<string> // add constructor for null data responses
        {
            Success = true,
            Message = Messages.ClassDeletedSuccessfully
        };

    }

    public async Task<ServiceResponse<GetClassDto>> UpdateClass(int classId, UpdateClassDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var classEntity = await _classRepository.GetClassByIdAsync(classId);
        if (classEntity == null)
        {
            return new ServiceResponse<GetClassDto>
            {
                Success = false,
                Message = Messages.ClassNotFound
            };
        }
        
        var isInstructor = classEntity.Instructor.UserId == user.Id;
        if (!isInstructor)
        {
            return new ServiceResponse<GetClassDto> 
            {
                Success = false,
                Message = Messages.ClassCan_tBeAccessed
            };
        }

        _mapper.Map(dto, classEntity);
        classEntity.UpdatedAt = DateTime.UtcNow;

        await _classRepository.UpdateClassAsync(classEntity);
        
        var responseDto = _mapper.Map<GetClassDto>(classEntity);
        // responseDto.InstructorFullName = $"{user.FirstName} {user.LastName}";
        return new ServiceResponse<GetClassDto> // add constructor for null data responses
        {
            Data = responseDto,
            Success = true,
            Message = Messages.ClassUpdatedSuccessfully
        };
    }
}