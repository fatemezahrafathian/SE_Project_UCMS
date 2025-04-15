using AutoMapper;
using UCMS.DTOs;
using UCMS.DTOs.ClassDto;
using UCMS.Models;
using UCMS.Repositories;
using UCMS.Repositories.ClassRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.ClassService.Abstraction;
using UCMS.Services.ImageService;

namespace UCMS.Services.ClassService;

public class ClassService: IClassService
{
    private readonly IClassRepository _classRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IInstructorRepository _instructorRepository;
    private readonly IImageService _imageService;
    

    public ClassService(IClassRepository classRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor, IInstructorRepository instructorRepository, IImageService imageService)
    {
        _classRepository = classRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _instructorRepository = instructorRepository;
        _imageService = imageService;
    }

    public async Task<ServiceResponse<GetClassForInstructorDto>> CreateClass(CreateClassDto dto) // check dates
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var newClass = _mapper.Map<Class>(dto);
        newClass.InstructorId = user!.Instructor!.Id;

        newClass.ClassCode = await GenerateUniqueClassCodeAsync();

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
        
        var isInstructorOfClass = classEntity.Instructor.UserId == user!.Id;
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
        
        // var isStudentOfClass = _studentClassService.isStudentOfClass(classId, user!.Student!.Id);
        // if (!isStudentOfClass)
        // {
        //     return new ServiceResponse<GetClassForStudentDto> 
        //     {
        //         Success = false,
        //         Message = Messages.ClassCan_tBeAccessed
        //     };
        // }

        var responseDto = _mapper.Map<GetClassForStudentDto>(classEntity);
        // responseDto.StudentCount = await _studentClassService.GetStudentClassCount(); // _studentClassService or repository
        return new ServiceResponse<GetClassForStudentDto>
        {
            Data = responseDto,
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
    
    public async Task<ServiceResponse<List<GetClassPreviewForInstructorDto>>> FilterClassesOfInstructor(PaginatedFilterClassForInstructorDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        // filtering
        
        // paginating
        
        // var clsDto = _mapper.Map<List<GetClassPreviewForInstructorDto>>(classList);

        return new ServiceResponse<List<GetClassPreviewForInstructorDto>> // add constructor for null data responses
        {
            Data = null,
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

        var isInstructor = classEntity.Instructor.UserId == user!.Id;
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
        
        var isInstructor = classEntity.Instructor.UserId == user!.Id;
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
}