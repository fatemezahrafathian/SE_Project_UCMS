using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.DTOs.PhaseSubmissionDto;
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


    public async Task AddRangeStudentTeamPhaseAsync(List<StudentTeamPhase> studentTeamPhases)
    {
        await _context.StudentTeamPhases.AddRangeAsync(studentTeamPhases);
        await _context.SaveChangesAsync();
    }

    public async Task<StudentTeamPhase?> GetStudentTeamPhaseAsync(int studentId, int phaseId)
    {
        return await _context.StudentTeamPhases.FirstOrDefaultAsync(stp =>
            stp.StudentTeam.StudentId == studentId && stp.PhaseId == phaseId);
    }

    public async Task<StudentTeamPhase?> GetStudentTeamPhaseByIdAsync(int studentTeamPhaseId)
    {
        return await _context.StudentTeamPhases.Where(stp => stp.Id == studentTeamPhaseId)
            .Include(stp => stp.Phase)
            .ThenInclude(p => p.Project)
            .ThenInclude(p => p.Class)
            .FirstOrDefaultAsync();
    }

    public async Task<StudentTeamPhase?> GetStudentTeamPhaseByStudentNumber(int phaseId, string studentNumber)
    {
        return await _context.StudentTeamPhases.Where(stp =>
                stp.PhaseId == phaseId && stp.StudentTeam.Student.StudentNumber == studentNumber)
            .Include(stp => stp.StudentTeam)
            .FirstOrDefaultAsync();
    }

    public async Task<List<StudentTeamPhase>> GetStudentTeamPhasesByPhaseAndTeamIdAsync(int phaseId, int teamId)
    {
        return await _context.StudentTeamPhases
            .Where(stp => stp.PhaseId == phaseId && stp.StudentTeam.TeamId == teamId)
            .Include(stp => stp.StudentTeam)
            .ThenInclude(st => st.Student)
            .ThenInclude(s => s.User)
            .OrderBy(stp=>stp.StudentTeam.Student.User.LastName + " " + stp.StudentTeam.Student.User.FirstName)
            .ToListAsync();
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

    public async Task UpdateStudentTeamPhaseAsync(StudentTeamPhase studentTeamPhase)
    {
        _context.StudentTeamPhases.UpdateRange(studentTeamPhase);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRangeStudentTeamPhaseAsync(List<StudentTeamPhase> studentTeamPhases)
    {
        _context.StudentTeamPhases.UpdateRange(studentTeamPhases);
        await _context.SaveChangesAsync();
    }
}