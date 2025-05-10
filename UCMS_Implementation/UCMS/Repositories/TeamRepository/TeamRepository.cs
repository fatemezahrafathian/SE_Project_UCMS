using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.Models;
using UCMS.Repositories.TeamRepository.Abstraction;

namespace UCMS.Repositories.TeamRepository;

public class TeamRepository: ITeamRepository
{
    private readonly DataContext _context;

    public TeamRepository(DataContext context)
    {
        _context = context;
    }

    public async Task AddTeamAsync(Team team)
    {
        await _context.Teams.AddAsync(team);
        await _context.SaveChangesAsync();
    }

    public async Task<Team?> GetTeamByTeamIdAsync(int teamId)
    {
        return await _context.Teams
            .FirstOrDefaultAsync(t => t.Id == teamId);
    }

    public async Task<Team?> GetTeamForInstructorByTeamIdAsync(int teamId)
    {
        return await _context.Teams
            .Where(t => t.Id == teamId)
            .Include(t => t.StudentTeams)
            .ThenInclude(st => st.Student)
            .ThenInclude(s => s.User)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Team>> GetTeamsForInstructorByProjectIdAsync(int projectId)
    {
        return await _context.Teams
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.StudentTeams)
            .ThenInclude(st => st.Student)
            .ThenInclude(s => s.User)
            .ToListAsync();
    }

    public async Task DeleteTeamAsync(Team team)
    {
        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateTeamAsync(Team team)
    {
        _context.Teams.Update(team);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsTeamForInstructor(int teamId, int instructorId)
    {
        return await _context.Teams
            .Where(t => t.Id == teamId)
            .AnyAsync(t =>
                t.Project.Class.InstructorId == instructorId);
    }
}