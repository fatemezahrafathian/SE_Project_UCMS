using UCMS.Models;

namespace UCMS.Repositories.StudentRepository.Abstraction
{
    public interface IStudentRepository
    {
        Task AddStudentAsync(Student student);
        Task<Student?> GetStudentByUserIdAsync(int userId);
        Task<Student?> GetStudentByIdAsync(int studentId);
        Task UpdateStudentAsync(Student student);

        Task<List<Student>> GetAllStudentsAsync();
        Task<bool> SaveChangesAsync();
    }
}
