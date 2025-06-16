using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.Models;
using UCMS.Repositories.PhaseRepository.Abstraction;

namespace UCMS.Repositories.PhaseRepository;

public class PhaseRepository:IPhaseRepository
{
    private readonly DataContext _context;
    public PhaseRepository(DataContext context)
    {
        _context = context;
    }
    public async Task AddAsync(Phase phase)
    {
        await _context.Phases.AddAsync(phase);
        await _context.SaveChangesAsync();
    }
    public async Task<Phase?> GetPhaseByIdAsync(int phaseId)
    {
        return await _context.Phases
            .Include(p => p.Project)
            .ThenInclude(pr => pr.Class)
            .FirstOrDefaultAsync(p => p.Id == phaseId);
    }
    public async Task<List<Phase>> GetPhasesByProjectIdAsync(int projectId)
    {
        return await _context.Phases
            .Where(p => p.ProjectId == projectId)
            .ToListAsync();
    }
    public async Task UpdateAsync(Phase phase)
    {
        _context.Phases.Update(phase);
        await _context.SaveChangesAsync();
    }
    public async Task<bool> ExistsWithTitleExceptIdAsync(string title, int projectId, int phaseIdToExclude)
    {
        return await _context.Phases
            .AnyAsync(p => p.Title == title && p.ProjectId == projectId && p.Id != phaseIdToExclude);
    }
    public async Task DeleteAsync(Phase phase)
    {
        _context.Phases.Remove(phase);
        await _context.SaveChangesAsync();
    }
    public async Task<List<Phase>> GetPhasesCloseDeadLines(DateTime lowerBound, DateTime upperBound,CancellationToken stoppingToken)
    {
        return await _context.Phases
            .Include(p => p.Project)
                .ThenInclude(pr => pr.Class.ClassStudents)
                .ThenInclude(cs => cs.Student.User)
            .Where(p => p.EndDate >= lowerBound && p.EndDate <= upperBound)
            .ToListAsync(stoppingToken);
    }
    public async Task<List<Phase>> GetPhasesCloseStartDate(DateTime lowerBound, DateTime upperBound,CancellationToken stoppingToken)
    {
        return await _context.Phases
            .Include(p => p.Project)
            .ThenInclude(pr => pr.Class.ClassStudents)
            .ThenInclude(cs => cs.Student.User)
            .Where(p => p.StartDate >= lowerBound && p.StartDate <= upperBound)
            .ToListAsync(stoppingToken);
    }
}