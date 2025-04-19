using UCMS.DTOs;
using UCMS.DTOs.Student;

namespace UCMS.Services.StudentService.Abstraction
{
    public interface IStudentService
    {
        Task<ServiceResponse<GetStudentDto>> EditStudentAsync(int userId, EditStudentDto editStudentDto);
    }
}
