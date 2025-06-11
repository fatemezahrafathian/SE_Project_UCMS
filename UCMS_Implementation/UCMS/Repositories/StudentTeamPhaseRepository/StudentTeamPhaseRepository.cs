using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.Models;
using UCMS.Repositories.StudentTeamPhaseRepository.Abstraction;

namespace UCMS.Repositories.StudentTeamPhaseRepository;

public class StudentTeamPhaseRepository: IStudentTeamPhaseRepository
{
    private readonly DataContext _context;

    public StudentTeamPhaseRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<StudentTeamPhase?> GetStudentTeamPhaseAsync(int studentId, int phaseId)
    {
        return await _context.StudentTeamPhases.FirstOrDefaultAsync(stp =>
            stp.StudentTeam.StudentId == studentId && stp.PhaseId == phaseId);
    }

    public async Task<StudentTeamPhase?> GetStudentTeamPhaseWithRelationAsync(int studentId, int phaseId)
    {
        return await _context.StudentTeamPhases.Where(stp =>
                stp.StudentTeam.StudentId == studentId && stp.PhaseId == phaseId)
            .Include(stp => stp.StudentTeam)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> AnyStudentTeamPhaseAsync(int studentId, int teamId, int phaseId)
    {
        return await _context.StudentTeamPhases.AnyAsync(stp =>
            stp.PhaseId == phaseId && stp.StudentTeam.StudentId == studentId && stp.StudentTeam.TeamId==teamId);
    }

    public async Task<StudentTeamPhase?> GetStudentTeamPhaseWithRelationAsync(int studentId, int teamId, int phaseId)
    {
        return await _context.StudentTeamPhases.FirstOrDefaultAsync(stp =>
            stp.PhaseId == phaseId && stp.StudentTeam.TeamId == teamId && stp.StudentTeam.StudentId == studentId);
    }
}