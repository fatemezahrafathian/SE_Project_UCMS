using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.DTOs.ExerciseSubmissionDto;
using UCMS.DTOs.PhaseSubmissionDto;
using UCMS.Models;
using UCMS.Repositories.ExerciseSubmissionRepository.Abstraction;

namespace UCMS.Repositories.ExerciseSubmissionRepository;

public class ExerciseSubmissionRepository: IExerciseSubmissionRepository
{
    private readonly DataContext _context;

    public ExerciseSubmissionRepository(DataContext context)
    {
        _context = context;
    }

    public async Task AddExerciseSubmissionAsync(ExerciseSubmission exerciseSubmission)
    {
        await _context.ExerciseSubmissions.AddAsync(exerciseSubmission);
        await _context.SaveChangesAsync();
    }

    public async Task<ExerciseSubmission?> GetExerciseSubmissionForInstructorByIdAsync(int exerciseSubmissionId)
    {
        return await _context.ExerciseSubmissions.Where(es => es.Id == exerciseSubmissionId)
            .Include(es => es.Exercise)
            .ThenInclude(e => e.Class)
            .FirstOrDefaultAsync();
    }

    public async Task<ExerciseSubmission?> GetExerciseSubmissionByIdAsync(int exerciseSubmissionId)
    {
        return await _context.ExerciseSubmissions.FirstOrDefaultAsync(es => es.Id == exerciseSubmissionId);
    }

    public async Task<List<ExerciseSubmission>> GetExerciseSubmissionsAsync(int exerciseId)
    {
        return await _context.ExerciseSubmissions
            .Where(es => es.ExerciseId == exerciseId && es.IsFinal)
            .Include(es=>es.Student)
            .ThenInclude(s=>s.User)
            .ToListAsync();
    }

    public async Task<List<ExerciseSubmission>> GetExerciseSubmissionsForInstructorByExerciseIdAsync(int exerciseId,
        SortExerciseSubmissionByForInstructorOption sortBy, SortOrderOption sortOrder)
    {
        IQueryable<ExerciseSubmission> query = _context.ExerciseSubmissions
            .Where(e => e.ExerciseId == exerciseId && e.IsFinal)
            .Include(e => e.Student)
            .ThenInclude(s => s.User);

        bool ascending = sortOrder == SortOrderOption.Ascending;

        switch (sortBy)
        {
            case SortExerciseSubmissionByForInstructorOption.Date:
                query = ascending 
                    ? query.OrderBy(ps => ps.SubmittedAt)
                    : query.OrderByDescending(ps => ps.SubmittedAt);
                break;
            
            case SortExerciseSubmissionByForInstructorOption.StudentName:
                query = ascending 
                    ? query.OrderBy(ps =>ps.Student.User.LastName + " " + ps.Student.User.FirstName)
                    : query.OrderByDescending(ps => ps.Student.User.LastName + " " + ps.Student.User.FirstName);
                break;

            case SortExerciseSubmissionByForInstructorOption.StudentNumber:
                query = ascending 
                    ? query.OrderBy(ps => ps.Student.StudentNumber)
                    : query.OrderByDescending(ps => ps.Student.StudentNumber);
                break;

            case SortExerciseSubmissionByForInstructorOption.None:
            default:
                break;
        }

        return await query.ToListAsync();
    }

    public async Task<List<ExerciseSubmission>> GetExerciseSubmissionsForStudentByExerciseIdAsync(int studentId, int exerciseId, SortExerciseSubmissionByForStudentOption sortBy,
        SortOrderOption sortOrder)
    {
        IQueryable<ExerciseSubmission> query = _context.ExerciseSubmissions
            .Where(e => e.ExerciseId == exerciseId && e.StudentId==studentId)
            .Include(e => e.Student)
            .ThenInclude(s => s.User);

        bool ascending = sortOrder == SortOrderOption.Ascending;

        switch (sortBy)
        {
            case SortExerciseSubmissionByForStudentOption.Date:
                query = ascending 
                    ? query.OrderBy(ps => ps.SubmittedAt)
                    : query.OrderByDescending(ps => ps.SubmittedAt);
                break;
            
            case SortExerciseSubmissionByForStudentOption.None:
            default:
                break;
        }

        return await query.ToListAsync();
    }

    public async Task<ExerciseSubmission?> GetFinalExerciseSubmissionsAsync(int studentId, int exerciseId)
    {
        return await _context.ExerciseSubmissions.FirstOrDefaultAsync(es =>
            es.StudentId == studentId && es.ExerciseId == exerciseId && es.IsFinal==true);
    }

    public async Task UpdateExerciseSubmissionAsync(ExerciseSubmission exerciseSubmission)
    {
        _context.ExerciseSubmissions.Update(exerciseSubmission);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRangeExerciseSubmissionAsync(List<ExerciseSubmission> exerciseSubmissions)
    {
        _context.ExerciseSubmissions.UpdateRange(exerciseSubmissions);
        await _context.SaveChangesAsync();
    }

    public async Task<ExerciseSubmission?> GetExerciseSubmissionByStudentNumber(int exerciseId, string studentNumber)
    {
        return await _context.ExerciseSubmissions.Where(es => es.ExerciseId==exerciseId && es.Student.StudentNumber == studentNumber && es.IsFinal)
            .Include(es=>es.Student)
            .ThenInclude(s=>s.User)
            .FirstOrDefaultAsync();
    }
}