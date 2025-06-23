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
    
    public async Task AddClassAsync(Class cls)
    {
        await _context.Classes.AddAsync(cls);
        await _context.SaveChangesAsync();
    }
    
    public async Task<Class?> GetClassByIdAsync(int id)
    {
        return await _context.Classes
            .Include(c=>c.ClassStudents)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Class?> GetInstructorClassByClassIdAsync(int id)
    {
        return await _context.Classes
            .Where(c => c.Id == id)
            .Include(c => c.Schedules)
            .FirstOrDefaultAsync(); 
    }
    
    public async Task<Class?> GetStudentClassByClassIdAsync(int id)
    {
        return await _context.Classes
            .Where(c => c.Id == id)
            .Include(c => c.Instructor)
            .ThenInclude(i => i.User)
            .Include(c => c.Schedules)
            .FirstOrDefaultAsync();
    }

    public async Task<Class?> GetClassWithEntriesAsync(int classId)
    {
        return await _context.Classes.Where(c => c.Id == classId)
            .Include(c => c.Projects)
            .ThenInclude(p => p.Phases)
            .Include(c => c.Exercises)
            .Include(c => c.Exams)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Class>> FilterStudentClassesWithRelations(int studentId, string? title)
    {
        var query = GetClassesWithRelations(studentId);
        if (!string.IsNullOrWhiteSpace(title))
            query = FilterClassesByTitle(query, title);

        return await query.ToListAsync();
    }
    
    private IQueryable<Class> GetClassesWithRelations(int studentId)
    {
        return _context.Classes
            .Where(c =>
                _context.ClassStudents.Any(cs => cs.ClassId == c.Id && cs.StudentId == studentId))
            .Include(c => c.Projects)
            .ThenInclude(p => p.Phases)
            .Include(c => c.Exercises)
            .Include(c => c.Exams)
            .OrderBy(c => c.Title);

    }
    
    public async Task<Class?> GetClassWithRelationsAsync(int studentId, int classId)
    {
        return await _context.Classes.Where(c=>c.Id==classId && _context.ClassStudents
                .Any(cs => cs.ClassId == c.Id && cs.StudentId == studentId))
            .Include(c=>c.ClassStudents)
            .ThenInclude(cs=>cs.Student)
            .ThenInclude(s=>s.User)
            .Include(c => c.Projects)
            .ThenInclude(p => p.Phases)
            .Include(c => c.Exercises)
            .Include(c => c.Exams)
            .FirstOrDefaultAsync();
    }

    public async Task<Class?> FilterClassStudentsWithRelations(int classId, string? fullName, string? studentNumber)
    {
        var cls = await GetClassWithRelations(classId);
        if (cls == null) return cls;
        if (!string.IsNullOrWhiteSpace(fullName))
            cls.ClassStudents = cls.ClassStudents
                .Where(cs =>
                    cs.Student.User.LastName != null &&
                    cs.Student.User.FirstName != null &&
                    (cs.Student.User.LastName + " " + cs.Student.User.FirstName)
                    .Contains(fullName))
                .ToList();

        if (!string.IsNullOrWhiteSpace(studentNumber))
            cls.ClassStudents = cls.ClassStudents
                .Where(cs => cs.Student.StudentNumber != null && cs.Student.StudentNumber
                    .Contains(studentNumber))
                .ToList();

        cls.ClassStudents = cls.ClassStudents.OrderBy(cs => cs.Student.User.LastName + " " + cs.Student.User.FirstName).ToList();
        return cls;

    }
    
    private async Task<Class?> GetClassWithRelations(int classId)
    {
        return await _context.Classes
            .Where(c => c.Id == classId)
            .Include(c => c.ClassStudents)
            .ThenInclude(cs => cs.Student)
            .ThenInclude(s => s.User)
            .Include(c => c.Projects)
            .ThenInclude(p => p.Phases)
            .Include(c => c.Exercises)
            .Include(c => c.Exams)
            .FirstOrDefaultAsync();

    }

    public async Task DeleteClassAsync(Class cls)
    {
        _context.Classes.Remove(cls);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateClassAsync(Class cls)
    {
        _context.Classes.Update(cls);
        await _context.SaveChangesAsync();
    }

    public IQueryable<Class> FilterInstructorClassesByInstructorIdAsync(int instructorId, string? title, bool? isActive)
    {
        var query = GetClassesByInstructorId(instructorId);
        if (!string.IsNullOrWhiteSpace(title))
            query = FilterClassesByTitle(query, title);
        if (isActive.HasValue)
            query = FilterClassesByIsActive(query, isActive.Value);

        return query;
    }

    private IQueryable<Class> GetClassesByInstructorId(int instructorId)
    {
        return _context.Classes
            .Include(c => c.ClassStudents)
            .Where(c => c.InstructorId == instructorId);
    }

    private IQueryable<Class> FilterClassesByTitle(IQueryable<Class> query, string title)
    {
        return query.Where(c => c.Title.Contains(title));
    }

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

    public async Task<Class?> GetClassByTokenAsync(string classCode)
    {
        return await _context.Classes
            .Where(c => c.ClassCode == classCode)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ClassCodeExistsAsync(string code)
    {
        return await _context.Classes.AnyAsync(c => c.ClassCode == code);
    }

}