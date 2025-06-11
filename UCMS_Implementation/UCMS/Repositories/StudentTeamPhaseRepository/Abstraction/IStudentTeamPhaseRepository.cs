using UCMS.Models;

namespace UCMS.Repositories.StudentTeamPhaseRepository.Abstraction;

public interface IStudentTeamPhaseRepository
{
    Task<StudentTeamPhase?> GetStudentTeamPhaseAsync(int studentId, int phaseId); 
    Task<StudentTeamPhase?> GetStudentTeamPhaseWithRelationAsync(int studentId, int phaseId);
    Task<bool> AnyStudentTeamPhaseAsync(int studentId, int teamId, int phaseId);
    Task<StudentTeamPhase?> GetStudentTeamPhaseWithRelationAsync(int studentId, int teamId, int phaseId);
}