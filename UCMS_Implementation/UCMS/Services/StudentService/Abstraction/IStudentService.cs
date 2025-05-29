using UCMS.DTOs;
using UCMS.DTOs.Student;

namespace UCMS.Services.StudentService.Abstraction
{
    public interface IStudentService
    {
        Task<ServiceResponse<StudentProfileDto>> GetStudentProfileById(int userId);
        Task<ServiceResponse<GetStudentDto>> GetSpecializedInfo();
        Task<ServiceResponse<StudentProfileDto>> GetCurrentStudent();
        Task<ServiceResponse<List<StudentPreviewDto>>> GetAllStudents();
        Task<ServiceResponse<GetStudentDto>> EditStudentAsync(EditStudentDto editStudentDto);
    }
}
