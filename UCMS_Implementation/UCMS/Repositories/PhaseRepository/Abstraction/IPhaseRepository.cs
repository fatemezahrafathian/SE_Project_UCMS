using UCMS.Models;

namespace UCMS.Repositories.PhaseRepository.Abstraction;

public interface IPhaseRepository
{
    Task AddAsync(Phase phase);
    Task<Phase?> GetPhaseByIdAsync(int phaseId);
    Task<Phase?> GetPhaseSimpleByIdAsync(int phaseId);
    Task<Phase?> GetPhaseWithRelationsByIdAsync(int phaseId);
    Task<List<Phase>> GetPhasesByProjectIdAsync(int projectId);
    Task UpdateAsync(Phase phase);
    Task<bool> ExistsWithTitleExceptIdAsync(string title, int projectId, int phaseIdToExclude);
    Task DeleteAsync(Phase phase);
    Task<List<Phase>> GetPhasesCloseDeadLines(DateTime lowerBound, DateTime upperBound,CancellationToken stoppingToken);

    Task<List<Phase>> GetPhasesCloseStartDate(DateTime lowerBound, DateTime upperBound,CancellationToken stoppingToken);


}