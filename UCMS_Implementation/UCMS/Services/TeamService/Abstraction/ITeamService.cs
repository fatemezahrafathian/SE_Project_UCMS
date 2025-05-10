using UCMS.DTOs;
using UCMS.DTOs.TeamDto;

namespace UCMS.Services.TeamService.Abstraction;

public interface ITeamService
{
    Task<ServiceResponse<GetTeamForInstructorDto>> CreateTeam(CreateTeamDto dto);
    Task<ServiceResponse<GetTeamForInstructorDto>> GetTeamForInstructor(int teamId);
    Task<ServiceResponse<List<GetTeamPreviewForInstructorDto>>> GetProjectTeamsForInstructor(int projectId);
    Task<ServiceResponse<string>> DeleteTeam(int teamId); 
    Task<ServiceResponse<GetTeamForInstructorDto>> UpdateTeamPartial(int teamId, PatchTeamDto dto);
}