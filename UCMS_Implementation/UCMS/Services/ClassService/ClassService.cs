using AutoMapper;
using UCMS.DTOs;
using UCMS.DTOs.ClassDto;
using UCMS.Models;
using UCMS.Repositories;
using UCMS.Repositories.ClassRepository.Abstraction;
using UCMS.Repositories.InstructorRepository.Abstraction;
using UCMS.Repositories.UserRepository.Abstraction;
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
    private readonly IInstructorRepository _instructorRepository;
    private readonly IImageService _imageService;
    private readonly IPasswordService _passwordService;
    private readonly IUserRepository _userRepository;
    

    public ClassService(IClassRepository classRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor, IInstructorRepository instructorRepository, IImageService imageService, IPasswordService passwordService, IUserRepository userRepository)
    {
        _classRepository = classRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _instructorRepository = instructorRepository;
        _imageService = imageService;
        _passwordService = passwordService;
        _userRepository = userRepository;
    }

    public async Task<ServiceResponse<GetClassForInstructorDto>> CreateClass(CreateClassDto dto) // check dates
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var newClass = _mapper.Map<Class>(dto);
        newClass.InstructorId = user!.Instructor!.Id;

        if (dto.StartDate.HasValue || dto.EndDate.HasValue)
        {
            if (dto.StartDate.HasValue && dto.EndDate.HasValue)
            {
                if (dto.StartDate.Value > dto.EndDate.Value)
                {
                    return new ServiceResponse<GetClassForInstructorDto>
                    {
                        Success = false,
                        Message = Messages.StartDateCanNotBeLaterThanEndDatte
                    };
                }
            }

            if (dto.StartDate.HasValue)
            {
                if (dto.StartDate.Value < DateOnly.FromDateTime(DateTime.UtcNow))
                {
                    return new ServiceResponse<GetClassForInstructorDto>
                    {
                        Success = false,
                        Message = Messages.StartDateCanNotBeInPast
                    };
                }
            }

            if (dto.EndDate.HasValue)
            {
                if (dto.EndDate.Value < DateOnly.FromDateTime(DateTime.UtcNow))
                {
                    return new ServiceResponse<GetClassForInstructorDto>
                    {
                        Success = false,
                        Message = Messages.EndDateCanNotBeInPast
                    };
                }
            }
        }

        
        
        if (dto.ProfileImage != null)
        {
            if (!_imageService.IsValidImageExtension(dto.ProfileImage))
            {
                return new ServiceResponse<GetClassForInstructorDto>
                {
                    Success = false,
                    Message = Messages.InvalidFormat
                };
            }

            if (!_imageService.IsValidImageSize(dto.ProfileImage))
            {
                return new ServiceResponse<GetClassForInstructorDto>
                {
                    Success = false,
                    Message = Messages.InvalidSize
                };
            }
            
            var imageUrl = await _imageService.SaveImageAsync(dto.ProfileImage, "images/classes"); // get from appsetting
            newClass.ProfileImageUrl = imageUrl;
        }

        if (!_passwordService.IsPasswordValid(dto.Password))
        {
            return new ServiceResponse<GetClassForInstructorDto> {Success = false, Message = Messages.PasswordNotStrong};
        }
        
        newClass.ClassCode = await GenerateUniqueClassCodeAsync();
        
        newClass.PasswordSalt = _passwordService.CreateSalt();
        newClass.PasswordHash = await _passwordService.HashPasswordAsync(dto.Password, newClass.PasswordSalt);

        await _classRepository.AddClassAsync(newClass);

        var responseDto = _mapper.Map<GetClassForInstructorDto>(newClass);
        
        return new ServiceResponse<GetClassForInstructorDto>
        {
            Data = responseDto,
            Message = Messages.ClassCreatedSuccessfully,
            Success = true
        };
    }

    public async Task<ServiceResponse<GetClassForInstructorDto>> GetClassForInstructor(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var classEntity = await _classRepository.GetClassForInstructorAsync(classId);
        if (classEntity == null)
        {
            return new ServiceResponse<GetClassForInstructorDto>
            {
                Success = false,
                Message = Messages.ClassNotFound
            };
        }
        
        var isInstructorOfClass = classEntity.InstructorId == user!.Instructor!.Id;
        if (!isInstructorOfClass)
        {
            return new ServiceResponse<GetClassForInstructorDto> 
            {
                Success = false,
                Message = Messages.ClassCan_tBeAccessed
            };
        }
        
        var responseDto = _mapper.Map<GetClassForInstructorDto>(classEntity);
        // responseDto.StudentCount = await _studentClassService.GetStudentClassCount(); // _studentClassService or repository
        return new ServiceResponse<GetClassForInstructorDto>
        {
            Data = responseDto,
            Message = Messages.ClassFetchedSuccessfully,
            Success = true
        };
    }

    public async Task<ServiceResponse<GetClassForStudentDto>> GetClassForStudent(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var classEntity = await _classRepository.GetClassForStudentAsync(classId);
        if (classEntity == null)
        {
            return new ServiceResponse<GetClassForStudentDto>
            {
                Success = false,
                Message = Messages.ClassNotFound
            };
        }
        
        var isStudentOfClass = await IsStudentOfClass(classId, user!.Student!.Id);
        if (!isStudentOfClass)
        {
            return new ServiceResponse<GetClassForStudentDto> 
            {
                Success = false,
                Message = Messages.ClassCan_tBeAccessed
            };
        }

        var responseDto = _mapper.Map<GetClassForStudentDto>(classEntity);
        responseDto.StudentCount = await GetStudentClassCount(classEntity.Id); // _studentClassService or repository
        return new ServiceResponse<GetClassForStudentDto>
        {
            Data = responseDto,
            Message = Messages.ClassFetchedSuccessfully,
            Success = true
        };

    }
    private async Task<int> GetStudentClassCount(int classId)
    {
        var classEntity = await _classRepository.GetClassByIdAsync(classId);
        return classEntity.ClassStudents.Count;
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
    
    public async Task<ServiceResponse<GetClassPageDto>> FilterClassesOfInstructor(PaginatedFilterClassForInstructorDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var classEntityList = await _classRepository.FilterAndPaginateClassesAsync(user!.Instructor!.Id, dto.Title, dto.IsActive, dto.Page,
            dto.PageSize);        
        
        var dtoPage = _mapper.Map<GetClassPageDto>(classEntityList);
        
        return new ServiceResponse<GetClassPageDto> // add constructor for null data responses
        {
            Data = dtoPage,
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

        var isInstructor = classEntity.InstructorId == user!.Instructor!.Id;
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
    public async Task<ServiceResponse<GetClassForInstructorDto>> PartialUpdateClass(int classId, PatchClassDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var classEntity = await _classRepository.GetClassForInstructorAsync(classId); // change the name of repository functions
        if (classEntity == null)
        {
            return new ServiceResponse<GetClassForInstructorDto>
            {
                Success = false,
                Message = Messages.ClassNotFound
            };
        }
        
        var isInstructor = classEntity.InstructorId == user!.Instructor!.Id;
        if (!isInstructor)
        {
            return new ServiceResponse<GetClassForInstructorDto> 
            {
                Success = false,
                Message = Messages.ClassCan_tBeAccessed
            };
        }

        _mapper.Map(dto, classEntity);
        if (dto.ProfileImage != null)
        {
            if (!_imageService.IsValidImageExtension(dto.ProfileImage))
            {
                return new ServiceResponse<GetClassForInstructorDto>
                {
                    Success = false,
                    Message = Messages.InvalidFormat
                };
            }

            if (!_imageService.IsValidImageSize(dto.ProfileImage))
            {
                return new ServiceResponse<GetClassForInstructorDto>
                {
                    Success = false,
                    Message = Messages.InvalidSize
                };
            }
            
            if (!string.IsNullOrWhiteSpace(classEntity.ProfileImageUrl))
            {
                _imageService.DeleteImage(classEntity.ProfileImageUrl);
            }



            if (dto.StartDate.HasValue)
            {
                if (dto.StartDate.Value < DateOnly.FromDateTime(classEntity.CreatedAt)) 
                {
                    return new ServiceResponse<GetClassForInstructorDto>
                    {
                        Success = false,
                        Message = Messages.StartDateCanNotBeEarlierThanCreationTime
                    };
                }
                // check with class elements
                if (dto.EndDate.HasValue)
                {
                    if (dto.StartDate.Value < dto.EndDate.Value)
                    {
                        return new ServiceResponse<GetClassForInstructorDto>
                        {
                            Success = false,
                            Message = Messages.StartDateCanNotBeLaterThanEndDatte
                        };
                    }
                }
                else
                {
                    if (dto.StartDate.Value < classEntity.EndDate.Value)
                    {
                        return new ServiceResponse<GetClassForInstructorDto>
                        {
                            Success = false,
                            Message = Messages.StartDateCanNotBeLaterThanEndDatte
                        };
                    }
                }
            }
            else
            {
                if (dto.EndDate.HasValue)
                {
                    // check with class elements
                }
            }
            
            
            
            
            // cloud
            var imageUrl = await _imageService.SaveImageAsync(dto.ProfileImage, "images/classes"); // get from appsetting
            classEntity.ProfileImageUrl = imageUrl;
        }
        classEntity.UpdatedAt = DateTime.UtcNow;

        await _classRepository.UpdateClassAsync(classEntity);
        
        var responseDto = _mapper.Map<GetClassForInstructorDto>(classEntity);
        return new ServiceResponse<GetClassForInstructorDto> // add constructor for null data responses
        {
            Data = responseDto,
            Success = true,
            Message = Messages.ClassUpdatedSuccessfully
        };
    }
    public async Task<bool> IsStudentOfClass(int classId, int studentId)
    {
        return await _classRepository.IsStudentOfClassAsync(classId, studentId);
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
        

        await _classRepository.AddStudentToClassAsync(classEntity.Id, user.Student.Id);
        
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

        var success = await _classRepository.RemoveStudentFromClassAsync(classId, user.Student.Id);
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

        var success = await _classRepository.RemoveStudentFromClassAsync(classId, StudentId);
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
        var students = await _classRepository.GetStudentsInClassAsync(classId);
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
        var students = await _classRepository.GetStudentsInClassAsync(classId);
        var dtoList = _mapper.Map<List<GetStudentsOfClassforStudentDto>>(students);
        return new ServiceResponse<List<GetStudentsOfClassforStudentDto>>
        {
            Success = true,
            Message = Messages.ListOfStudent,
            Data = dtoList
        };
    }

}