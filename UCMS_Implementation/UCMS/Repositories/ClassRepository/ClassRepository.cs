using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.DTOs;
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
    
    public async Task AddClassAsync(Class cls)
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
    
    
    public async Task<Page<Class>> FilterAndPaginateClassesAsync(int instructorId, string? title, bool? isActive, int page, int pageSize)
    {
        var query = _context.Classes
            .Where(c => c.InstructorId == instructorId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(c => c.Title.Contains(title));
        }

        if (isActive.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
    
            if (isActive.Value)
            {
                query = query.Where(c =>
                    (!c.StartDate.HasValue && !c.EndDate.HasValue) ||
                    (c.StartDate.HasValue && !c.EndDate.HasValue && c.StartDate.Value <= today) ||
                    (!c.StartDate.HasValue && c.EndDate.HasValue && c.EndDate.Value >= today) ||
                    (c.StartDate.HasValue && c.EndDate.HasValue && c.StartDate.Value <= today && c.EndDate.Value >= today));
            }
            else
            {
                query = query.Where(c =>
                    (c.StartDate.HasValue && c.StartDate.Value > today) ||
                    (c.EndDate.HasValue && c.EndDate.Value < today));
            }
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            // .Include(c => c.Instructor)
            // .ThenInclude(i => i.User)
            .ToListAsync();

        return new Page<Class>()
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
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