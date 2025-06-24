using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.Models;
using UCMS.Repositories.StudentExamRepository.Abstraction;

namespace UCMS.Repositories.StudentExamRepository;

public class StudentExamRepository: IStudentExamRepository
{
    private readonly DataContext _context;

    public StudentExamRepository(DataContext context)
    {
        _context = context;
    }

    public async Task AddRangeStudentExamAsync(List<StudentExam> studentExams)
    {
        _context.AddRange(studentExams);
        await _context.SaveChangesAsync();
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

    public async Task UpdateRangeStudentExamAsync(List<StudentExam> studentExams)
    {
        _context.StudentExams.UpdateRange(studentExams);
        await _context.SaveChangesAsync();
    }


}