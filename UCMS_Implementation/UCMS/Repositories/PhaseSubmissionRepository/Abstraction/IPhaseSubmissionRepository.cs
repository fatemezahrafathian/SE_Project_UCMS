using UCMS.DTOs.TeamPhaseDto;
using UCMS.Models;

namespace UCMS.Repositories.TeamPhaseRepository.Abstraction;

public interface IPhaseSubmissionRepository
{
    Task AddPhaseSubmissionAsync(PhaseSubmission phaseSubmission);
    Task<PhaseSubmission?> GetPhaseSubmissionForInstructorByIdAsync(int submissionId);
    Task<PhaseSubmission?> GetPhaseSubmissionForStudentByIdAsync(int submissionId);
    Task<List<PhaseSubmission>> GetPhaseSubmissionsForInstructorByPhaseIdAsync(int phaseId, SortPhaseSubmissionByForInstructorOption sortBy, SortOrderOption sortOrder);
    Task<List<PhaseSubmission>> GetPhaseSubmissionsForStudentByPhaseIdAsync(int teamId, int phaseId, SortPhaseSubmissionByForStudentOption sortBy, SortOrderOption sortOrder);
    Task<List<PhaseSubmission>> GetPhaseSubmissionsAsync(int phaseId);
    Task<PhaseSubmission?> GetFinalPhaseSubmissionsAsync(int phaseId, int teamId);
    Task UpdatePhaseSubmissionAsync(PhaseSubmission phaseSubmission);
}