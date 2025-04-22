using UCMS.DTOs.Student;

namespace UCMS.Services.StudentService.Abstraction
{
    public interface IStudentService
    {
        Task<bool> EditStudentAsync(int userId, EditStudentDto editStudentDto);
    }
}
