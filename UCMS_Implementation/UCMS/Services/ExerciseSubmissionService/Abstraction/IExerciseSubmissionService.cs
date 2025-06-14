using UCMS.DTOs;
using UCMS.DTOs.ExerciseSubmissionDto;

namespace UCMS.Services.ExerciseSubmissionService.Abstraction;

public interface IExerciseSubmissionService
{
    Task<ServiceResponse<string>> CreateExerciseSubmission(int exerciseId, CreateExerciseSubmissionDto dto);
    Task<ServiceResponse<FileDownloadDto>> GetExerciseSubmissionFileForInstructor(int exerciseSubmissionId);
    Task<ServiceResponse<FileDownloadDto>> GetExerciseSubmissionFileForStudent(int exerciseSubmissionId);
    Task<ServiceResponse<List<FileDownloadDto>>> GetExerciseSubmissionFiles(int exerciseId);
    Task<ServiceResponse<List<GetExerciseSubmissionPreviewForInstructorDto>>> GetExerciseSubmissionsForInstructor(SortExerciseSubmissionsForInstructorDto dto);
    Task<ServiceResponse<List<GetExerciseSubmissionPreviewForStudentDto>>> GetExerciseSubmissionsForStudent(SortExerciseSubmissionsStudentDto sto);
    Task<ServiceResponse<string>> UpdateFinalSubmission(int exerciseSubmissionId);
}