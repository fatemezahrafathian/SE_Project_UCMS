using UCMS.DTOs;
using UCMS.DTOs.ExerciseSubmissionDto;

namespace UCMS.Services.ExerciseSubmissionService.Abstraction;

public interface IExerciseSubmissionService
{
    Task<ServiceResponse<GetExerciseSubmissionPreviewForStudentDto>> CreateExerciseSubmission(int exerciseId, CreateExerciseSubmissionDto dto);
    Task<ServiceResponse<FileDownloadDto>> GetExerciseSubmissionFileForInstructor(int exerciseSubmissionId);
    Task<ServiceResponse<FileDownloadDto>> GetExerciseSubmissionFileForStudent(int exerciseSubmissionId);
    Task<ServiceResponse<FileDownloadDto>> GetExerciseSubmissionFiles(int exerciseId);
    Task<ServiceResponse<List<GetExerciseSubmissionPreviewForInstructorDto>>> GetExerciseSubmissionsForInstructor(SortExerciseSubmissionsForInstructorDto dto);
    Task<ServiceResponse<List<GetExerciseSubmissionPreviewForStudentDto>>> GetExerciseSubmissionsForStudent(SortExerciseSubmissionsStudentDto dto);
    Task<ServiceResponse<FileDownloadDto>> GetExerciseScoreTemplateFile(int exerciseId);
    Task<ServiceResponse<string>> UpdateFinalExerciseSubmission(int exerciseSubmissionId);
    Task<ServiceResponse<string>> UpdateExerciseSubmissionScore(int exerciseSubmissionId, UpdateExerciseSubmissionScoreDto dto);
    Task<ServiceResponse<List<GetScoreFileValidationResultDto>>> UpdateExerciseSubmissionScores(int exerciseId, IFormFile scoreFile);

}