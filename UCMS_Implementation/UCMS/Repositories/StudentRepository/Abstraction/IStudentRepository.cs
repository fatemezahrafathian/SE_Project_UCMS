using UCMS.Models;

namespace UCMS.Repositories.StudentRepository.Abstraction
{
    public interface IStudentRepository
    {
        Task AddStudentAsync(Student student);
        Task<Student> GetStudentByUserIdAsync(int userId);
        Task UpdateStudentAsync(Student student);
        Task<bool> SaveChangesAsync();
    }
}
