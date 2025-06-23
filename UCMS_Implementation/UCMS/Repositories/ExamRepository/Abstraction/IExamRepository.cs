using UCMS.Models;

namespace UCMS.Repositories.ExamRepository.Abstraction;

public interface IExamRepository
{
    Task AddAsync(Exam exam);
    Task<Exam?> GetExamByIdAsync(int examId);
    Task<List<Exam>> GetExamsByClassIdAsync(int classId);
    Task<StudentExam?> GetStudentExamsByStudentNumberAsync(int examId, string studentNumber);
    Task<StudentExam?> GetStudentExamAsync(int studentId, int examId);
    Task UpdateAsync(Exam exercise);
    Task UpdateRangeStudentExamAsync(List<StudentExam> studentExams);
    Task<bool> ExistsWithTitleExceptIdAsync(string title, int classId, int examIdToExclude);
    Task DeleteAsync(Exam exam);
    Task<List<Exam>> GetExamsByStudentIdAsync(int studentId);
    Task<List<Exam>> GetExamsByInstructorIdAsync(int instructorId);
    Task<List<Exam>> GetExamsCloseDeadLines(DateTime lowerBound, DateTime upperBound, CancellationToken stoppingToken);
}