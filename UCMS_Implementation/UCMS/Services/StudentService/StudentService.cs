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

        public StudentService(IStudentRepository studentRepository, IMapper mapper)
        {
            _studentRepository = studentRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<GetStudentDto>> EditStudentAsync(int userId, EditStudentDto editStudentDto)
        {
            var student = await _studentRepository.GetStudentByUserIdAsync(userId);
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
