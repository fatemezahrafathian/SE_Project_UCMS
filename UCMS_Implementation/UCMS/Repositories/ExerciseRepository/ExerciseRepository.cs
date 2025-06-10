using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.Models;
using UCMS.Repositories.ExerciseRepository.Abstraction;

namespace UCMS.Repositories.ExerciseRepository;

public class ExerciseRepository:IExerciseRepository
{
    private readonly DataContext _context;
    public ExerciseRepository(DataContext context)
    {
        _context = context;
    }
    public async Task AddAsync(Exercise exercise)
    {
        await _context.Exercises.AddAsync(exercise);
        await _context.SaveChangesAsync();
    }
    public async Task<Exercise?> GetExerciseByIdAsync(int exerciseId)
    {
        return await _context.Exercises
            .Include(p => p.Class)
            .FirstOrDefaultAsync(p => p.Id == exerciseId);
    }
    public async Task<List<Exercise>> GetExercisesByClassIdAsync(int classId)
    {
        return await _context.Exercises
            .Where(p => p.ClassId == classId)
            .ToListAsync();
    }
    public async Task UpdateAsync(Exercise exercise)
    {
        _context.Exercises.Update(exercise);
        await _context.SaveChangesAsync();
    }
    public async Task<bool> ExistsWithTitleExceptIdAsync(string title, int classId, int exerciseIdToExclude)
    {
        return await _context.Exercises
            .AnyAsync(p => p.Title == title && p.ClassId == classId && p.Id != exerciseIdToExclude);
    }
    public async Task DeleteAsync(Exercise exercise)
    {
        _context.Exercises.Remove(exercise);
        await _context.SaveChangesAsync();
    }
    public async Task<List<Exercise>> GetExercisesByStudentIdAsync(int studentId)
    {
        return await _context.Exercises
            .Include(e => e.Class)
            .Where(e => e.Class.ClassStudents.Any(cs => cs.StudentId == studentId))
            .ToListAsync();
    }
    public async Task<List<Exercise>> GetExercisesByInstructorIdAsync(int instructorId)
    {
        return await _context.Exercises
            .Include(e => e.Class)
            .Where(e => e.Class.InstructorId == instructorId)
            .ToListAsync();
    }
    
}