using UCMS.DTOs;
using UCMS.Models;

namespace UCMS.Repositories.ClassRepository.Abstraction;

public interface IClassRepository
{
    Task AddClassAsync(Class? cls);
    Task<bool> IsClassCodeExistAsync(string code);
    Task<Class?> GetClassByIdAsync(int id);
    Task<Class?> GetClassForInstructorAsync(int id);
    Task<Class?> GetClassForStudentAsync(int id);
    Task<List<Class>> GetClassesByInstructorAsync(int instructorId);
    Task<Page<Class>> FilterAndPaginateClassesAsync(int instructorId, string? title, bool? isActive,
        int page, int pageSize);
    Task DeleteClassAsync(Class? cls); 
    Task UpdateClassAsync(Class? cls);
    Task<Class?> GetClassByTokenAsync(string classCode);
    Task<bool> IsStudentOfClassAsync(int classId, int studentId);
    Task AddStudentToClassAsync(int classId, int studentId);
    Task<bool> RemoveStudentFromClassAsync(int classId, int studentId);
    Task<List<Student>> GetStudentsInClassAsync(int classId);
}