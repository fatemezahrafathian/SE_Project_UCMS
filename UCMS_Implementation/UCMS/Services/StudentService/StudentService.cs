using AutoMapper;
using UCMS.DTOs;
using UCMS.DTOs.Student;
using UCMS.Models;
using UCMS.Repositories.StudentRepository.Abstraction;
using UCMS.Repositories.UserRepository;
using UCMS.Repositories.UserRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.StudentService.Abstraction;
using UCMS.Services.Utils;

namespace UCMS.Services.StudentService
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<StudentService> _logger;
        private readonly UrlBuilder _urlBuilder;
        private readonly IUserRepository _userRepository;

        public StudentService(IStudentRepository studentRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILogger<StudentService> logger, UrlBuilder urlBuilder, IUserRepository userRepository)
        {
            _studentRepository = studentRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _userRepository = userRepository;
            _urlBuilder = urlBuilder;
        }

        public async Task<ServiceResponse<StudentProfileDto>> GetStudentProfileById(int userId)
        {
            User? user = await _userRepository.GetUserByIdAsync(userId);
            Student? student = await _studentRepository.GetStudentByUserIdAsync(userId);

            return BuildStudentProfileOutput(student, userId);
        }

        public async Task<ServiceResponse<StudentProfileDto>> GetCurrentStudent()
        {
            var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
            Student? student = await _studentRepository.GetStudentByUserIdAsync(user.Id);

            return BuildStudentProfileOutput(student, user.Id);
        }

        private ServiceResponse<StudentProfileDto> BuildStudentProfileOutput(Student? student, int userId)
        {
            if (student == null)
            {
                return new ServiceResponse<StudentProfileDto>
                {
                    Success = false,
                    Message = String.Format(Messages.UserNotFound, userId)
                };
            }

            StudentProfileDto responseStudent = _mapper.Map<StudentProfileDto>(student);
            responseStudent.ProfileImagePath = _urlBuilder.BuildUrl(_httpContextAccessor, responseStudent.ProfileImagePath);
            return new ServiceResponse<StudentProfileDto>
            {
                Data = responseStudent,
                Success = true,
                Message = string.Format(Messages.UserFound, student.UserId)
            };
        }

        public async Task<ServiceResponse<GetStudentDto>> GetSpecializedInfo()
        {
            var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
            var student = await _studentRepository.GetStudentByUserIdAsync(user.Id);

            if (student == null)
            {
                return new ServiceResponse<GetStudentDto>
                {
                    Success = false,
                    Message = String.Format(Messages.UserNotFound, student.UserId)
                };
            }

            GetStudentDto responseStudent = _mapper.Map<GetStudentDto>(student);
            return new ServiceResponse<GetStudentDto>
            {
                Data = responseStudent,
                Success = true,
                Message = string.Format(Messages.UserFound, user.Id)
            };
        }

        public async Task<ServiceResponse<GetStudentDto>> EditStudentAsync(EditStudentDto editStudentDto)
        {
            var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
            var student = await _studentRepository.GetStudentByUserIdAsync(user.Id);

            if (student == null)
                return new ServiceResponse<GetStudentDto>
                {
                    Success = false,
                    Message = String.Format(Messages.UserNotFound, user.Id)
                };

            Student updatedStudent = _mapper.Map(editStudentDto, student);
            await _studentRepository.UpdateStudentAsync(updatedStudent);
            _logger.LogInformation("Student {userId} updated successfully", user.Id);

            GetStudentDto responseStudent = _mapper.Map<GetStudentDto>(updatedStudent);
            return new ServiceResponse<GetStudentDto>
            {
                Data = responseStudent,
                Success = true,
                Message = string.Format(Messages.UpdateUser, user.Id)
            };
        }

        public async Task<ServiceResponse<List<StudentPreviewDto>>> GetAllStudents()
        {
            List<Student> students = await _studentRepository.GetAllStudentsAsync();
            var response = _mapper.Map<List<StudentPreviewDto>>(students);

            return new ServiceResponse<List<StudentPreviewDto>>
            {
                Data = response,
                Success = true,
                Message = Messages.AllUsersFetchedSuccessfully
            };
        }
    }
}
