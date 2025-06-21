using UCMS.Models;

namespace UCMS.Repositories.ExamRepository.Abstraction;

public interface IExamRepository
{
    Task AddAsync(Exam exam);
    Task<Exam?> GetExamByIdAsync(int examId);
    Task<List<Exam>> GetExamsByClassIdAsync(int classId);
    Task<StudentExam?> GetStudentExamsByStudentNumberAsync(int examId, string studentNumber);
    Task UpdateAsync(Exam exercise);
    Task UpdateRangeStudentExamAsync(List<StudentExam> studentExams);
    Task<bool> ExistsWithTitleExceptIdAsync(string title, int classId, int examIdToExclude);
    Task DeleteAsync(Exam exam);
}