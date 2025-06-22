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

    public async Task<Exam?> GetSimpleExamWithoutRelationsByIdAsync(int examId)
    {
        return await _context.Exams.FirstOrDefaultAsync(e => e.Id == examId);
    }

    public async Task<List<Exam>> GetExamsByClassIdAsync(int classId)
    {
        return await _context.Exams
            .Where(p => p.ClassId == classId)
            .ToListAsync();
    }

    public async Task<StudentExam?> GetStudentExamsByStudentNumberAsync(int examId, string studentNumber)
    {
        return await _context.StudentExams.FirstOrDefaultAsync(se =>
            se.ExamId == examId && se.Student.StudentNumber == studentNumber);
    }

    public async Task<StudentExam?> GetStudentExamAsync(int studentId, int examId)
    {
        return await _context.StudentExams.FirstOrDefaultAsync(se => se.StudentId == studentId && se.ExamId == examId);
    }

    public async Task UpdateAsync(Exam exam)
    {
        _context.Exams.Update(exam);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRangeStudentExamAsync(List<StudentExam> studentExams)
    {
        _context.StudentExams.UpdateRange(studentExams);
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
}
