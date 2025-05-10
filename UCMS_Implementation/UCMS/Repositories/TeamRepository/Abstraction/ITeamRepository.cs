using UCMS.Models;

namespace UCMS.Repositories.TeamRepository.Abstraction;

public interface ITeamRepository
{
    Task AddTeamAsync(Team team);
    Task<bool> IsTeamForInstructor(int teamId, int instructorId);
    Task<Team?> GetTeamByTeamIdAsync(int teamId);
    Task<Team?> GetTeamForInstructorByTeamIdAsync(int teamId);
    Task<List<Team>> GetTeamsForInstructorByProjectIdAsync(int projectId);
    Task DeleteTeamAsync(Team team);
    Task UpdateTeamAsync(Team team);
}