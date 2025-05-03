using AutoMapper;
using UCMS.DTOs;
using UCMS.DTOs.ClassDto;
using UCMS.Extensions;
using UCMS.Factories;
using UCMS.Models;
using UCMS.Repositories.ClassRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.ClassService.Abstraction;
using UCMS.Services.PasswordService.Abstraction;

namespace UCMS.Services.ClassService;

public class StudentClassService: IStudentClassService
{
    private readonly IClassRepository _classRepository;
    private readonly IStudentClassRepository _studentClassRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;

    public StudentClassService(IClassRepository classRepository, IStudentClassRepository studentClassRepository, IHttpContextAccessor httpContextAccessor, IPasswordService passwordService, IMapper mapper)
    {
        _classRepository = classRepository;
        _studentClassRepository = studentClassRepository;
        _httpContextAccessor = httpContextAccessor;
        _passwordService = passwordService;
        _mapper = mapper;
    }

    private async Task<bool> IsStudentOfClass(int classId, int studentId)
    {
        return await _studentClassRepository.IsStudentOfClassAsync(classId, studentId);
    }
    public async Task<ServiceResponse<JoinClassResponseDto>> JoinClassAsync(JoinClassRequestDto request)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var classEntity = await _classRepository.GetClassByTokenAsync(request.ClassCode);
        
        if (classEntity == null)
        {
            return new ServiceResponse<JoinClassResponseDto>
            {
                Success = false,
                Message = Messages.ClassNotFound
            };
        }
        if (!await _passwordService.VerifyPasswordAsync(request.Password, classEntity.PasswordSalt, classEntity.PasswordHash))
            return new ServiceResponse<JoinClassResponseDto> { Success = false, Message = Messages.WrongPasswordMessage };
        var alreadyJoined = await IsStudentOfClass(classEntity.Id, user.Student.Id);
        if (alreadyJoined)
        {
            return new ServiceResponse<JoinClassResponseDto>
            {
                Success = false,
                Message = Messages.AlreadyJoinedClass
            };
        }
        
        var now = DateOnly.FromDateTime(DateTime.Now);
        if (classEntity.StartDate.HasValue)
        {
            if (now < classEntity.StartDate)
            {
                return new ServiceResponse<JoinClassResponseDto>
                {
                    Success = false,
                    Message = Messages.ClassCurrentlyNotActive
                };
            }
        }
        if (classEntity.EndDate.HasValue)
        {
            if (now > classEntity.EndDate)
            {
                return new ServiceResponse<JoinClassResponseDto>
                {
                    Success = false,
                    Message = Messages.ClassCurrentlyNotActive
                };
            }
        }
        

        await _studentClassRepository.AddStudentToClassAsync(classEntity.Id, user.Student.Id);
        
        return new ServiceResponse<JoinClassResponseDto>
        {
            Success = true,
            Message = Messages.ClassJoinedSuccessfully,
            Data = new JoinClassResponseDto(){classId = classEntity.Id}
        };
    }
    
    public async Task<ServiceResponse<bool>> LeaveClassAsync(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var classEntity = await _classRepository.GetClassByIdAsync(classId);
        if (classEntity == null)
            return new ServiceResponse<bool> { Success = false, Message = Messages.ClassNotFound };
        
        var isStudentOfClass = await IsStudentOfClass(classEntity.Id, user.Student.Id);
        if (!isStudentOfClass)
        {
            return new ServiceResponse<bool>
            {
                Success = false,
                Message = Messages.StudentNotInClass
            };
        }

        var success = await _studentClassRepository.RemoveStudentFromClassAsync(classId, user.Student.Id);
        if (!success)
            return new ServiceResponse<bool> { Success = false, Message = Messages.LeftClassNotSuccessfully };

        return new ServiceResponse<bool> { Success = true, Message = Messages.LeftClassSuccessfully };
    }

    public async Task<ServiceResponse<bool>> RemoveStudentFromClassAsync(int classId, int StudentId)
    {
        var instructor = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var classEntity = await _classRepository.GetClassByIdAsync(classId);
        if (classEntity == null)
            return new ServiceResponse<bool> { Success = false, Message = Messages.ClassNotFound };

        if (instructor.Id != classEntity.InstructorId)
        {
            return new ServiceResponse<bool>
            {
                Success = false,
                Message = Messages.UnauthorizedAccess
            };
        }

        var isStudentOfClass = await IsStudentOfClass(classEntity.Id, StudentId);
        if (!isStudentOfClass)
        {
            return new ServiceResponse<bool>
            {
                Success = false,
                Message = Messages.StudentNotInClass
            };
        }

        var success = await _studentClassRepository.RemoveStudentFromClassAsync(classId, StudentId);
        if (!success)
            return new ServiceResponse<bool>
                { Success = false, Message = Messages.RemoveStudentFromClassNotSuccessfully };

        return new ServiceResponse<bool> { Success = true, Message = Messages.RemoveStudentFromClassSuccessfully };
    }
    
    public async Task<ServiceResponse<List<GetStudentsOfClassforInstructorDto>>> GetStudentsOfClassByInstructorAsync(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var classEntity = await _classRepository.GetClassByIdAsync(classId);
        if (classEntity == null)
            return new ServiceResponse<List<GetStudentsOfClassforInstructorDto>> { Success = false, Message = Messages.ClassNotFound };

        if (classEntity.Instructor.UserId != user.Id)
        {
            return new ServiceResponse<List<GetStudentsOfClassforInstructorDto>>
            {
                Success = false,
                Message = Messages.UnauthorizedAccess
            };
        }
        var students = await _studentClassRepository.GetStudentsInClassAsync(classId);
        var dtoList = _mapper.Map<List<GetStudentsOfClassforInstructorDto>>(students);
        return new ServiceResponse<List<GetStudentsOfClassforInstructorDto>>
        {
            Success = true,
            Message = Messages.ListOfStudent,
            Data = dtoList
        };
    }
    public async Task<ServiceResponse<List<GetStudentsOfClassforStudentDto>>> GetStudentsOfClassByStudentAsync(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var classEntity = await _classRepository.GetClassByIdAsync(classId);
        if (classEntity == null)
            return new ServiceResponse<List<GetStudentsOfClassforStudentDto>> { Success = false, Message = Messages.ClassNotFound };

        var isStudentOfClass = await IsStudentOfClass(classEntity.Id, user.Student.Id);
        if (!isStudentOfClass)
        {
            return new ServiceResponse<List<GetStudentsOfClassforStudentDto>>
            {
                Success = false,
                Message = Messages.StudentNotInClass
            };
        }
        var students = await _studentClassRepository.GetStudentsInClassAsync(classId);
        var dtoList = _mapper.Map<List<GetStudentsOfClassforStudentDto>>(students);
        return new ServiceResponse<List<GetStudentsOfClassforStudentDto>>
        {
            Success = true,
            Message = Messages.ListOfStudent,
            Data = dtoList
        };
    }

    public async Task<int> GetStudentClassCount(int classId)
    {
        var classEntity = await _classRepository.GetClassByIdAsync(classId);
        return classEntity.ClassStudents.Count;
    }
    public async Task<ServiceResponse<GetClassForStudentDto>> GetClassForStudent(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var classEntity = await _classRepository.GetStudentClassByClassIdAsync(classId);
        if (classEntity == null)
        {
            return ServiceResponseFactory.Failure<GetClassForStudentDto>(
                Messages.ClassNotFound);
        }

        // var isStudentOfClass = await _studentClassService.IsStudentOfClass(classId, user!.Student!.Id);
        // if (!isStudentOfClass)
        // {
        //     return new ServiceResponse<GetClassForStudentDto> 
        //     {
        //         Success = false,
        //         Message = Messages.ClassCan_tBeAccessed
        //     };
        // }

        var responseDto = _mapper.Map<GetClassForStudentDto>(classEntity);
        // responseDto.StudentCount = await GetStudentClassCount(classEntity.Id);
        return ServiceResponseFactory.Success(responseDto, Messages.ClassFetchedSuccessfully);
    }
    public async Task<ServiceResponse<GetClassPageForStudentDto>> GetClassesForStudent(PaginatedFilterClassForStudentDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var classEntityQueryable = _studentClassRepository.FilterStudentClassesByStudentIdAsync(user!.Student!.Id, dto.Title, dto.IsActive, dto.InstructorName);        
        var classEntityList = await classEntityQueryable.PaginateAsync(dto.Page, dto.PageSize);

        var responseDto = _mapper.Map<GetClassPageForStudentDto>(classEntityList);
        
        return ServiceResponseFactory.Success(responseDto, Messages.ClassesRetrievedSuccessfully); // ClassesFetchedSuccessfully
    }

}