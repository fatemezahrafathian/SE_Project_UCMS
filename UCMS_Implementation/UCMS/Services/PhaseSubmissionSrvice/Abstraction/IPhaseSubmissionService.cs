using UCMS.DTOs;
using UCMS.DTOs.ExerciseSubmissionDto;
using UCMS.DTOs.PhaseSubmissionDto;

namespace UCMS.Services.TeamPhaseSrvice;

public interface IPhaseSubmissionService
{
    Task<ServiceResponse<GetPhaseSubmissionPreviewForStudentDto>> CreatePhaseSubmission(int phaseId, CreatePhaseSubmissionDto dto);
    Task<ServiceResponse<FileDownloadDto>> GetPhaseSubmissionFileForInstructor(int phaseSubmissionId);
    Task<ServiceResponse<FileDownloadDto>> GetPhaseSubmissionFileForStudent(int phaseSubmissionId);
    Task<ServiceResponse<FileDownloadDto>> GetPhaseSubmissionFiles(int phaseId);
    Task<ServiceResponse<List<GetPhaseSubmissionPreviewForInstructorDto>>> GetPhaseSubmissionsForInstructor(SortPhaseSubmissionsForInstructorDto dto);
    Task<ServiceResponse<List<GetPhaseSubmissionPreviewForStudentDto>>> GetPhaseSubmissionsForStudent(SortPhaseSubmissionsStudentDto sto);
    Task<ServiceResponse<List<GetStudentTeamPhasePreviewDto>>> GetTeamPhaseMembers(int phaseId, int teamId);
    Task<ServiceResponse<FileDownloadDto>> GetPhaseScoreTemplateFile(int phaseId);
    Task<ServiceResponse<string>> UpdateFinalPhaseSubmission(int phaseSubmissionId);
    Task<ServiceResponse<string>> UpdatePhaseSubmissionScore(int studentTeamPhaseId, UpdatePhaseSubmissionScoreDto dto);
    Task<ServiceResponse<List<GetScoreFileValidationResultDto>>> UpdatePhaseSubmissionScores(int phaseId, IFormFile scoreFile);

}