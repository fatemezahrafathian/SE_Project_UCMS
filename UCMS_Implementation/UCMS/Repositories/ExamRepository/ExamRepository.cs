using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.Models;
using UCMS.Repositories.ExamRepository.Abstraction;

namespace UCMS.Repositories.ExamRepository;

public class ExamRepository:IExamRepository
{
    private readonly DataContext _context;
    public ExamRepository(DataContext context)
    {
        _context = context;
    }
    public async Task AddAsync(Exam exam)
    {
        await _context.Exams.AddAsync(exam);
        await _context.SaveChangesAsync();
    }
    public async Task<Exam?> GetExamByIdAsync(int examId)
    {
        return await _context.Exams
            .Include(p => p.Class)
            .FirstOrDefaultAsync(p => p.Id == examId);
    }

    public async Task<Exam?> GetExamWithClassRelationsByIdAsync(int examId)
    {
        return await _context.Exams
            .Where(e => e.Id == examId)
            .Include(e=>e.Class)
            .ThenInclude(c=>c.ClassStudents)
            .ThenInclude(cs=>cs.Student)
            .ThenInclude(cs=>cs.User)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Exam>> GetExamsByClassIdAsync(int classId)
    {
        return await _context.Exams
            .Where(p => p.ClassId == classId)
            .ToListAsync();
    }
    
    public async Task UpdateAsync(Exam exam)
    {
        _context.Exams.Update(exam);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsWithTitleExceptIdAsync(string title, int classId, int examIdToExclude)
    {
        return await _context.Exams
            .AnyAsync(p => p.Title == title && p.ClassId == classId && p.Id != examIdToExclude);
    }
    
    public async Task DeleteAsync(Exam exam)
    {
        _context.Exams.Remove(exam);
        await _context.SaveChangesAsync();
    }
    public async Task<List<Exam>> GetExamsByStudentIdAsync(int studentId)
    {
        return await _context.Exams
            .Include(e => e.Class)
            .Where(e => e.Class.ClassStudents.Any(cs => cs.StudentId == studentId))
            .ToListAsync();
    }
    public async Task<List<Exam>> GetExamsByInstructorIdAsync(int instructorId)
    {
        return await _context.Exams
            .Include(e => e.Class)
            .Where(e => e.Class.InstructorId == instructorId)
            .ToListAsync();
    }
    public async Task<List<Exam>> GetExamsCloseDeadLines(DateTime lowerBound, DateTime upperBound,CancellationToken stoppingToken)
    {
        return await _context.Exams
            .Include(e => e.Class.ClassStudents)
            .ThenInclude(cs => cs.Student.User)
            .Where(p => p.Date >= lowerBound && p.Date <= upperBound)
            .ToListAsync(stoppingToken);
    }
}
