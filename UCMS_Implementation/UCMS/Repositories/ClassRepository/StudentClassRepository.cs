using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.Models;
using UCMS.Repositories.ClassRepository.Abstraction;

namespace UCMS.Repositories.ClassRepository;

public class StudentClassRepository: IStudentClassRepository
{
    private readonly DataContext _context;

    public StudentClassRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<bool> IsStudentOfClassAsync(int classId, int studentId)
    {
        return await _context.ClassStudents
            .AnyAsync(cs => cs.ClassId == classId && cs.StudentId == studentId);
    }
    public async Task AddStudentToClassAsync(int classId, int studentId)
    {
        var cs = new ClassStudent
        {
            ClassId = classId,
            StudentId = studentId,
            JoinedAt = DateTime.UtcNow 
        };
        await _context.ClassStudents.AddAsync(cs);
        await _context.SaveChangesAsync();
    }
    public async Task<bool> RemoveStudentFromClassAsync(int classId, int studentId)
    {
        var classStudent = await _context.ClassStudents
            .FirstOrDefaultAsync(cs => cs.ClassId == classId && cs.StudentId == studentId);

        if (classStudent == null)
            return false;

        _context.ClassStudents.Remove(classStudent);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<List<Student>> GetStudentsInClassAsync(int classId)
    {
        return await _context.ClassStudents
            .Where(cs => cs.ClassId == classId)
            .Include(cs => cs.Student)
            .ThenInclude(s => s.User)
            .Select(cs => cs.Student)
            .ToListAsync();
    }

}