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
    public IQueryable<Class> FilterStudentClassesByStudentIdAsync(int studentId, string? title, bool? isActive)
    {
        var baseQuery = GetClassesByStudentId(studentId);
        if (!string.IsNullOrWhiteSpace(title))
            baseQuery = FilterClassesByInstructorNameAndTitle(baseQuery, title);
        
        if (isActive.HasValue)
            baseQuery = FilterClassesByIsActive(baseQuery, isActive.Value);

        return baseQuery;
    }

    public async Task<List<string?>> GetStudentNumbersOfClass(int classId)
    {
        return await _context.ClassStudents
            .Where(cs => cs.ClassId == classId)
            .Select(cs => cs.Student.StudentNumber)
            .Distinct()
            .ToListAsync();
    }

    public async Task<bool> AreStudentsInClassAsync(List<int> studentIds, int classId)
    {
        return await _context.ClassStudents
            .Where(cs => cs.ClassId == classId && studentIds.Contains(cs.Student.Id))
            .AnyAsync();
    }
    
    private IQueryable<Class> GetClassesByStudentId(int studentId)
    {
        return _context.Classes
            .Include(c=>c.Instructor)
            .Include(c=>c.Instructor.User)
            .Include(c => c.ClassStudents)
            .Where(c => c.ClassStudents.Any(cs => cs.StudentId == studentId));
    }
    private IQueryable<Class> FilterClassesByInstructorNameAndTitle(IQueryable<Class> query,string title)
    {
        return query.Where(c => c.Instructor.User.FirstName.Contains(title)  || c.Instructor.User.LastName.Contains(title) ||  c.Title.Contains(title));
    }
    // private IQueryable<Class> FilterClassesByInstructorName(IQueryable<Class> query, string instructorName)
    // {
    //     return query.Where(c => c.Instructor.User.FirstName.Contains(instructorName)  || c.Instructor.User.LastName.Contains(instructorName));
    // }
    // private IQueryable<Class> FilterClassesByTitle(IQueryable<Class> query, string title)
    // {
    //     return query.Where(c => c.Title.Contains(title));
    // }

    private IQueryable<Class> FilterClassesByIsActive(IQueryable<Class> query, bool isActive)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return isActive
            ? query.Where(c =>
                (!c.StartDate.HasValue && !c.EndDate.HasValue) ||
                (c.StartDate.HasValue && !c.EndDate.HasValue && c.StartDate.Value <= today) ||
                (!c.StartDate.HasValue && c.EndDate.HasValue && c.EndDate.Value >= today) ||
                (c.StartDate.HasValue && c.EndDate.HasValue && c.StartDate.Value <= today && c.EndDate.Value >= today))
            : query.Where(c =>
                (c.StartDate.HasValue && c.StartDate.Value > today) ||
                (c.EndDate.HasValue && c.EndDate.Value < today));
    }
    

}