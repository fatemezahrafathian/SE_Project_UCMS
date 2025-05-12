using UCMS.Models;

namespace UCMS.Repositories.ClassRepository.Abstraction;

public interface IStudentClassRepository
{
    Task<bool> IsStudentOfClassAsync(int classId, int studentId);
    Task AddStudentToClassAsync(int classId, int studentId);
    Task<bool> RemoveStudentFromClassAsync(int classId, int studentId);
    Task<List<Student>> GetStudentsInClassAsync(int classId);
    IQueryable<Class> FilterStudentClassesByStudentIdAsync(int studentId, string? title, bool? isActive,string? instructorName);
}