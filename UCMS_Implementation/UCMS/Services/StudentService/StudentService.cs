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

        public StudentService(IStudentRepository studentRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _studentRepository = studentRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResponse<GetStudentDto>> EditStudentAsync(EditStudentDto editStudentDto)
        {
            var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
            var student = await _studentRepository.GetStudentByUserIdAsync(user.Id);

            if (student == null)
                return new ServiceResponse<GetStudentDto>
                {
                    Success = false,
                    //Message = String.Format(Messages.UserNotFound, userId) FIXME
                };

            Student updateStudent = _mapper.Map(editStudentDto, student);
            await _studentRepository.UpdateStudentAsync(updateStudent);

            GetStudentDto responseStudent = _mapper.Map<GetStudentDto>(updateStudent);
            return new ServiceResponse<GetStudentDto>
            {
                Data = responseStudent,
                Success = true,
                //Message = message FIXME
            }; ;
        }
    }
}
