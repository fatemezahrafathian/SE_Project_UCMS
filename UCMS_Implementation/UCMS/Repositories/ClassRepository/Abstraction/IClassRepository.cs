using UCMS.DTOs;
using UCMS.Models;

namespace UCMS.Repositories.ClassRepository.Abstraction;

public interface IClassRepository
{
    Task AddClassAsync(Class cls);
    Task<Class?> GetClassByIdAsync(int id);
    Task<Class?> GetInstructorClassByClassIdAsync(int id);
    Task<Class?> GetStudentClassByClassIdAsync(int id);
    Task<Class?> GetClassWithEntriesAsync(int classId);
    Task<Class?> GetClassWithRelationsByIdAsync(int classId);
    Task<Class?> GetClassWithRelationsAsync(int studentId, int classId);
    Task<List<Class>> GetClassesWithRelationsAsync(int studentId);
    IQueryable<Class> FilterInstructorClassesByInstructorIdAsync(int instructorId, string? title, bool? isActive);
    Task DeleteClassAsync(Class cls); 
    Task UpdateClassAsync(Class cls);
    Task<bool> ClassCodeExistsAsync(string code);
    Task<Class?> GetClassByTokenAsync(string classCode);
}