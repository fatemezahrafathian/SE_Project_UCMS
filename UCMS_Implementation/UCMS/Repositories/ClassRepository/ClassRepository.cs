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