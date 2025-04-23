using UCMS.DTOs;
using UCMS.DTOs.Student;

namespace UCMS.Services.StudentService.Abstraction
{
    public interface IStudentService
    {
        //Task<ServiceResponse<GetStudentProfileDto>> GetStudentProfileById(int studentId);
        Task<ServiceResponse<GetStudentDto>> GetStudentById(int studentId);
        Task<ServiceResponse<StudentProfileDto>> GetCurrentStudent();
        Task<ServiceResponse<List<StudentPreviewDto>>> GetAllStudents();
        //Task<ServiceResponse<GetSstudentPreviewDto>> GetStudentPreviewById(int studentId);
        Task<ServiceResponse<GetStudentDto>> EditStudentAsync(EditStudentDto editStudentDto);
    }
}
