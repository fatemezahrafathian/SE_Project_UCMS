using UCMS.Models;

namespace UCMS.Repositories.StudentExamRepository.Abstraction;

public interface IStudentExamRepository
{
    Task AddRangeStudentExamAsync(List<StudentExam> studentExams);
    Task<StudentExam?> GetStudentExamsByStudentNumberAsync(int examId, string studentNumber);
    Task<StudentExam?> GetStudentExamAsync(int studentId, int examId);
    Task UpdateRangeStudentExamAsync(List<StudentExam> studentExams);

}