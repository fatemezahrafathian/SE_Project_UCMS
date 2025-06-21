using UCMS.Models;

namespace UCMS.Repositories.StudentTeamPhaseRepository.Abstraction;

public interface IStudentTeamPhaseRepository
{
    Task AddRangeStudentTeamPhaseAsync(List<StudentTeamPhase> studentTeamPhases);
    Task<StudentTeamPhase?> GetStudentTeamPhaseAsync(int studentId, int phaseId);
    Task<StudentTeamPhase?> GetStudentTeamPhaseByIdAsync(int studentTeamPhaseId);
    Task<StudentTeamPhase?> GetStudentTeamPhaseByStudentNumber(int phaseId, string studentNumber);
    Task<List<StudentTeamPhase>> GetStudentTeamPhasesByPhaseAndTeamIdAsync(int phaseId, int teamId);
    Task<StudentTeamPhase?> GetStudentTeamPhaseWithRelationAsync(int studentId, int phaseId);
    Task<bool> AnyStudentTeamPhaseAsync(int studentId, int teamId, int phaseId);
    Task<StudentTeamPhase?> GetStudentTeamPhaseWithRelationAsync(int studentId, int teamId, int phaseId);
    Task UpdateStudentTeamPhaseAsync(StudentTeamPhase studentTeamPhase);
    Task UpdateRangeStudentTeamPhaseAsync(List<StudentTeamPhase> studentTeamPhases);
    
    
    
}