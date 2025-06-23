using UCMS.DTOs;
using UCMS.DTOs.ExerciseSubmissionDto;

namespace UCMS.Services.StudentExamService.Abstraction;

public interface IStudentExamService
{
    Task<ServiceResponse<List<GetScoreFileValidationResultDto>>> UpdateExamScores(int examId, IFormFile scoreFile);
    Task<ServiceResponse<FileDownloadDto>> GetExamScoreTemplateFile(int exerciseId);

}