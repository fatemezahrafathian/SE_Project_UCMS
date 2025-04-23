using AutoMapper;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using UCMS.DTOs;
using UCMS.DTOs.Student;
using UCMS.Models;
using UCMS.Repositories.StudentRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.StudentService.Abstraction;

namespace UCMS.Services.StudentService
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<StudentService> _logger;

        public StudentService(IStudentRepository studentRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILogger<StudentService> logger)
        {
            _studentRepository = studentRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }


        public async Task<ServiceResponse<StudentProfileDto>> GetCurrentStudent()
        {
            var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
            Student? student = await _studentRepository.GetStudentByUserIdAsync(user.Id);

            if (student == null)
            {
                return new ServiceResponse<StudentProfileDto>
                {
                    Success = false,
                    Message = String.Format(Messages.UserNotFound, user.Id)
                };
            }

            StudentProfileDto responseStudent = _mapper.Map<StudentProfileDto>(student);
            return new ServiceResponse<StudentProfileDto>
            {
                Data = responseStudent,
                Success = true,
                Message = Messages.UserFound
            };
        }

        public async Task<ServiceResponse<GetStudentDto>> GetStudentById(int studentId)
        {
            Student? student = await _studentRepository.GetStudentByIdAsync(studentId);

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
                Message = Messages.UserFound
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
            _logger.LogInformation("Student {} updated successfully", user.Id);

            GetStudentDto responseStudent = _mapper.Map<GetStudentDto>(updatedStudent);
            return new ServiceResponse<GetStudentDto>
            {
                Data = responseStudent,
                Success = true,
                Message = Messages.UpdateUser
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
