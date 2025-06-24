using UCMS.Models;

namespace UCMS.Repositories.ExamRepository.Abstraction;

public interface IExamRepository
{
    Task AddAsync(Exam exam);
    Task<Exam?> GetExamByIdAsync(int examId);
    Task<Exam?> GetExamWithClassRelationsByIdAsync(int examId);
    Task<List<Exam>> GetExamsByClassIdAsync(int classId);
    Task UpdateAsync(Exam exercise);
    Task<bool> ExistsWithTitleExceptIdAsync(string title, int classId, int examIdToExclude);
    Task DeleteAsync(Exam exam);
    Task<List<Exam>> GetExamsByStudentIdAsync(int studentId);
    Task<List<Exam>> GetExamsByInstructorIdAsync(int instructorId);
    Task<List<Exam>> GetExamsCloseDeadLines(DateTime lowerBound, DateTime upperBound, CancellationToken stoppingToken);
}