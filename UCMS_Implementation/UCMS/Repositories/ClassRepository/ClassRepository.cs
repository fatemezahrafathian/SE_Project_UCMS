using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.Models;
using UCMS.Repositories.ClassRepository.Abstraction;

namespace UCMS.Repositories.ClassRepository;

public class ClassRepository: IClassRepository
{
    private readonly DataContext _context;

    public ClassRepository(DataContext context)
    {
        _context = context;
    }
    
    public async Task AddClassAsync(Class? cls)
    {
        await _context.Classes.AddAsync(cls);
        await _context.SaveChangesAsync();
    }
    
    public async Task<bool> IsClassCodeExistAsync(string code)
    {
        return await _context.Classes.AnyAsync(c => c.ClassCode == code);
    }

    public async Task<Class?> GetClassByIdAsync(int id)
    {
        return await _context.Classes
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id);
    }


    public async Task<Class?> GetClassForInstructorAsync(int id) // sync this
    {
        return await _context.Classes
            .Where(c => c.Id == id)
            .Include(c => c.Schedules)
            .FirstOrDefaultAsync(); 
    }

    public async Task<Class?> GetClassForStudentAsync(int id)
    {
        return await _context.Classes
            .Where(c => c.Id == id)
            .Include(c => c.Instructor)
            .ThenInclude(i => i.User)
            .Include(c => c.Schedules)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Class>> GetClassesByInstructorAsync(int instructorId)
    {
        return await _context.Classes
            .Where(c => c.InstructorId == instructorId)
            .Include(c => c.Schedules)
            .ToListAsync();
    }

    public async Task DeleteClassAsync(Class? cls)
    {
        _context.Classes.Remove(cls);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateClassAsync(Class? cls)
    {
        _context.Classes.Update(cls);
        await _context.SaveChangesAsync();
    }
    public async Task<Class?> GetClassByTokenAsync(string classCode)
    {
        return await _context.Classes
            .Where(c => c.ClassCode == classCode)
            .FirstOrDefaultAsync();
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