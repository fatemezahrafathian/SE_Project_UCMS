using UCMS.DTOs;
using UCMS.DTOs.PhaseSubmissionDto;

namespace UCMS.Services.TeamPhaseSrvice;

public interface IPhaseSubmissionService
{
    Task<ServiceResponse<string>> CreatePhaseSubmission(int phaseId, CreatePhaseSubmissionDto dto);
    Task<ServiceResponse<FileDownloadDto>> GetPhaseSubmissionFileForInstructor(int phaseSubmissionId);
    Task<ServiceResponse<FileDownloadDto>> GetPhaseSubmissionFileForStudent(int phaseSubmissionId);
    Task<ServiceResponse<List<FileDownloadDto>>> GetPhaseSubmissionFiles(int phaseId);
    Task<ServiceResponse<List<GetPhaseSubmissionPreviewForInstructorDto>>> GetPhaseSubmissionsForInstructor(SortPhaseSubmissionsForInstructorDto dto);
    Task<ServiceResponse<List<GetPhaseSubmissionPreviewForStudentDto>>> GetPhaseSubmissionsForStudent(SortPhaseSubmissionsStudentDto sto);
    Task<ServiceResponse<FileDownloadDto>> GetPhaseScoreTemplateFile(int phaseId);
    Task<ServiceResponse<string>> UpdateFinalPhaseSubmission(int phaseSubmissionId);
    Task<ServiceResponse<string>> UpdatePhaseSubmissionScore(int studentTeamPhaseId, UpdatePhaseSubmissionScoreDto dto);
    Task<ServiceResponse<string>> UpdatePhaseSubmissionScores(int phaseIdId, IFormFile scoreFile);

}