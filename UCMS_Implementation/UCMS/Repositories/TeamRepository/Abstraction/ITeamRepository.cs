using UCMS.Models;

namespace UCMS.Repositories.TeamRepository.Abstraction;

public interface ITeamRepository
{
    Task AddTeamAsync(Team team);
    Task AddTeamsAsync(List<Team> teams);
    Task<bool> IsTeamInInstructorClasses(int teamId, int instructorId);
    Task<bool> IsTeamInStudentClasses(int teamId, int studentId);
    Task<Team?> GetTeamByTeamIdAsync(int teamId);
    Task<Team?> GetTeamWithStudentTeamsByIdAsync(int teamId);
    Task<Team?> GetTeamForInstructorByTeamIdAsync(int teamId);
    Task<Team?> GetTeamForStudentByTeamIdAsync(int teamId);
    Task<List<Team>> GetTeamsByProjectIdAsync(int projectId);
    Task<List<Team>> GetTeamsWithRelationsByProjectIdAsync(int projectId);
    Task DeleteTeamAsync(Team team);
    Task UpdateTeamAsync(Team team);
    Task<List<string?>> GetStudentNumbersOfProjectTeams(int projectId);
}