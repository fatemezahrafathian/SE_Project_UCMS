using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.DTOs.PhaseSubmissionDto;
using UCMS.Repositories.PhaseSubmissionRepository.Abstraction;

namespace UCMS.Repositories.PhaseSubmissionRepository;

public class PhaseSubmissionRepository: IPhaseSubmissionRepository
{
    private readonly DataContext _context;

    public PhaseSubmissionRepository(DataContext context)
    {
        _context = context;
    }

    public async Task AddPhaseSubmissionAsync(PhaseSubmission phaseSubmission)
    {
        await _context.PhaseSubmissions.AddAsync(phaseSubmission);
        await _context.SaveChangesAsync();
    }

    public async Task<PhaseSubmission?> GetPhaseSubmissionForInstructorByIdAsync(int phaseSubmissionId)
    {
        return await _context.PhaseSubmissions.Where(ps => ps.Id == phaseSubmissionId)
            .Include(ps => ps.StudentTeamPhase)
            .ThenInclude(stp => stp.Phase)
            .ThenInclude(p => p.Project)
            .ThenInclude(p => p.Class)
            .FirstOrDefaultAsync();
    }

    public async Task<PhaseSubmission?> GetPhaseSubmissionForStudentByIdAsync(int phaseSubmissionId)
    {
        return await _context.PhaseSubmissions
            .Where(ps => ps.Id == phaseSubmissionId)
            .Include(ps => ps.StudentTeamPhase)
            .ThenInclude(stp => stp.StudentTeam)
            .FirstOrDefaultAsync();
    }

    public async Task<List<PhaseSubmission>> GetPhaseSubmissionsForInstructorByPhaseIdAsync(int phaseId, SortPhaseSubmissionByForInstructorOption sortBy,
        SortOrderOption sortOrder)
    {
        IQueryable<PhaseSubmission> query = _context.PhaseSubmissions
            .Where(ps => ps.StudentTeamPhase.PhaseId == phaseId && ps.IsFinal)
            .Include(ps => ps.StudentTeamPhase)
            .ThenInclude(stp => stp.StudentTeam)
            .ThenInclude(st => st.Team);

        bool ascending = sortOrder == SortOrderOption.Ascending;

        switch (sortBy)
        {
            case SortPhaseSubmissionByForInstructorOption.Date:
                query = ascending 
                    ? query.OrderBy(ps => ps.SubmittedAt)
                    : query.OrderByDescending(ps => ps.SubmittedAt);
                break;

            case SortPhaseSubmissionByForInstructorOption.TeamName:
                query = ascending 
                    ? query.OrderBy(ps => ps.StudentTeamPhase.StudentTeam.Team.Name)
                    : query.OrderByDescending(ps => ps.StudentTeamPhase.StudentTeam.Team.Name);
                break;

            case SortPhaseSubmissionByForInstructorOption.None:
            default:
                break;
        }

        return await query.ToListAsync();
    }

    public async Task<List<PhaseSubmission>> GetPhaseSubmissionsForStudentByPhaseIdAsync(int teamId, int phaseId, SortPhaseSubmissionByForStudentOption sortBy,
        SortOrderOption sortOrder)
    {
        IQueryable<PhaseSubmission> query = _context.PhaseSubmissions
            .Where(ps => ps.StudentTeamPhase.PhaseId == phaseId && ps.StudentTeamPhase.StudentTeam.Team.Id==teamId);
        
        bool ascending = sortOrder == SortOrderOption.Ascending;

        switch (sortBy)
        {
            case SortPhaseSubmissionByForStudentOption.Date:
                query = ascending 
                    ? query.OrderBy(ps => ps.SubmittedAt)
                    : query.OrderByDescending(ps => ps.SubmittedAt);
                break;
            
            case SortPhaseSubmissionByForStudentOption.None:
            default:
                break;
        }

        return await query.ToListAsync();
    }

    public async Task<List<PhaseSubmission>> GetPhaseSubmissionsAsync(int phaseId)
    {
        return await _context.PhaseSubmissions
            .Where(ps => ps.StudentTeamPhase.PhaseId == phaseId && ps.IsFinal).ToListAsync();
    }

    public async Task<PhaseSubmission?> GetFinalPhaseSubmissionsAsync(int teamId, int phaseId)
    {
        return await _context.PhaseSubmissions.FirstOrDefaultAsync(ps =>
            ps.StudentTeamPhase.PhaseId == phaseId && 
            ps.StudentTeamPhase.StudentTeam.TeamId == teamId &&
            ps.IsFinal == true);
    }

    public async Task UpdatePhaseSubmissionAsync(PhaseSubmission phaseSubmission)
    {
        _context.PhaseSubmissions.Update(phaseSubmission);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> AnyPhaseSubmissionForTeam(int teamId)
    {
        return await _context.PhaseSubmissions.AnyAsync(ps => ps.StudentTeamPhase.StudentTeam.TeamId == teamId);
    }
}