using UCMS.Models;

namespace UCMS.Repositories.ClassRepository.Abstraction;

public interface IClassRepository
{
    Task AddClassAsync(Class cls);
    Task<bool> IsClassCodeExistAsync(string code);
    Task<Class?> GetClassByIdAsync(int id);
    Task<Class?> GetClassForInstructorAsync(int id);
    Task<Class?> GetClassForStudentAsync(int id);
    Task<List<Class>> GetClassesByInstructorAsync(int instructorId);
    Task DeleteClassAsync(Class cls); 
    Task UpdateClassAsync(Class cls);
}