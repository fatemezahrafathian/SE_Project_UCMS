using UCMS.DTOs;
using UCMS.DTOs.TeamPhaseDto;

namespace UCMS.Services.TeamPhaseSrvice;

public interface IPhaseSubmissionService
{
    Task<ServiceResponse<string>> CreatePhaseSubmission(int phaseId, CreatePhaseSubmissionDto dto);
    Task<ServiceResponse<FileDownloadDto>> GetPhaseSubmissionFileForInstructor(int submissionId);
    Task<ServiceResponse<FileDownloadDto>> GetPhaseSubmissionFileForStudent(int submissionId);
    Task<ServiceResponse<List<FileDownloadDto>>> GetPhaseSubmissionFiles(int phaseId);
    Task<ServiceResponse<List<GetSubmissionPreviewForInstructorDto>>> GetPhaseSubmissionsForInstructor(SortPhaseSubmissionsForInsrtuctorDto dto);
    Task<ServiceResponse<List<GetSubmissionPreviewForStudentDto>>> GetPhaseSubmissionsForStudent(SortPhaseSubmissionsStudentDto sto);
    Task<ServiceResponse<string>> UpdateFinalSubmission(int submissionId);
    
}