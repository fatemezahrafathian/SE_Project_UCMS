using UCMS.DTOs;
using UCMS.DTOs.ExamDto;
using UCMS.DTOs.ExerciseSubmissionDto;

namespace UCMS.Services.ExamService.Abstraction;

public interface IExamService
{
    Task<ServiceResponse<GetExamForInstructorDto>> CreateExamAsync(int classId, CreateExamDto dto);
    Task<ServiceResponse<GetExamForInstructorDto>> GetExamByIdForInstructorAsync(int examId);
    Task<ServiceResponse<GetExamForInstructorDto>> UpdateExamAsync(int examId, PatchExamDto dto);
    Task<ServiceResponse<string>>  DeleteExamAsync(int examId);
    Task<ServiceResponse<List<GetExamForInstructorDto>>> GetExamsForInstructor(int classId);
    Task<ServiceResponse<GetExamForStudentDto>> GetExamByIdForStudentAsync(int examId);
    Task<ServiceResponse<List<GetExamForStudentDto>>> GetExamsForStudent(int classId);
    Task<ServiceResponse<List<GetScoreFileValidationResultDto>>> UpdateExamScores(int examId, IFormFile scoreFile);


}