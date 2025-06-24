using UCMS.Models;

namespace UCMS.Repositories.ClassRepository.Abstraction;

public interface IClassRepository
{
    Task AddClassAsync(Class cls);
    Task<Class?> GetClassByIdAsync(int id);
    Task<Class?> GetInstructorClassByClassIdAsync(int id);
    Task<Class?> GetStudentClassByClassIdAsync(int id);
    Task<Class?> GetClassWithEntriesAsync(int classId); 
    Task<Class?> GetClassWithRelationsAsync(int studentId, int classId); 
    Task<Class?> FilterClassStudentsWithRelations(int classId, string? fullName, string? studentNumber);
    IQueryable<Class> FilterInstructorClassesByInstructorIdAsync(int instructorId, string? title, bool? isActive); 
    Task<List<Class>> FilterStudentClassesWithRelations(int studentId, string? title);
    Task DeleteClassAsync(Class cls); 
    Task UpdateClassAsync(Class cls);
    Task<bool> ClassCodeExistsAsync(string code);
    Task<Class?> GetClassByTokenAsync(string classCode);
}