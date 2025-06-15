using UCMS.DTOs.PhaseSubmissionDto;

namespace UCMS.Repositories.PhaseSubmissionRepository.Abstraction;

public interface IPhaseSubmissionRepository
{
    Task AddPhaseSubmissionAsync(PhaseSubmission phaseSubmission);
    Task<PhaseSubmission?> GetPhaseSubmissionForInstructorByIdAsync(int phaseSubmissionId);
    Task<PhaseSubmission?> GetPhaseSubmissionForStudentByIdAsync(int phaseSubmissionId);
    Task<List<PhaseSubmission>> GetPhaseSubmissionsForInstructorByPhaseIdAsync(int phaseId, SortPhaseSubmissionByForInstructorOption sortBy, SortOrderOption sortOrder);
    Task<List<PhaseSubmission>> GetPhaseSubmissionsForStudentByPhaseIdAsync(int teamId, int phaseId, SortPhaseSubmissionByForStudentOption sortBy, SortOrderOption sortOrder);
    Task<List<PhaseSubmission>> GetPhaseSubmissionsAsync(int phaseId);
    Task<PhaseSubmission?> GetFinalPhaseSubmissionsAsync(int teamId, int phaseId);
    Task UpdatePhaseSubmissionAsync(PhaseSubmission phaseSubmission);
}