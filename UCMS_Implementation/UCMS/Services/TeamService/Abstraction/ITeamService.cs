using UCMS.DTOs;
using UCMS.DTOs.TeamDto;

namespace UCMS.Services.TeamService.Abstraction;

public interface ITeamService
{
    Task<ServiceResponse<GetTeamForInstructorDto>> CreateTeam(int peojectId, CreateTeamDto dto);
    Task<ServiceResponse<GetTeamForInstructorDto>> GetTeamForInstructor(int teamId); 
    Task<ServiceResponse<GetTeamForStudentDto>> GetTeamForStudent(int teamId);
    Task<ServiceResponse<List<GetTeamPreviewDto>>> GetProjectTeamsForInstructor(int projectId);
    Task<ServiceResponse<List<GetTeamPreviewDto>>> GetProjectTeamsForStudent(int projectId);
    Task<ServiceResponse<string>> DeleteTeam(int teamId); 
    Task<ServiceResponse<GetTeamForInstructorDto>> UpdateTeamPartial(int teamId, PatchTeamDto dto); 
    Task<ServiceResponse<GetTeamTemplateFileDto>> GetTeamTemplateFile(int projectId);
    Task<ServiceResponse<List<GetTeamFileValidationResultDto>>> CreateTeams(int projectId, IFormFile file); 
}