using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.DTOs.TeamDto;
using UCMS.Services.TeamService.Abstraction;

namespace UCMS.Controllers;
// add for student
// add create using file
// add validations
// clean code
[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }
    
    [RoleBasedAuthorization("Instructor")]
    [HttpPost("")]
    public async Task<IActionResult> CreateTeam([FromForm] CreateTeamDto dto)
    {
        var response = await _teamService.CreateTeam(dto);
        if (response.Success)
            return Ok(response);
        return BadRequest(response.Message);
    }
    
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("instructor/{teamId}")]
    public async Task<IActionResult> GetTeamForInstructor(int teamId)
    {
        var response = await _teamService.GetTeamForInstructor(teamId);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }

    [RoleBasedAuthorization("Instructor")]
    [HttpGet("instructor/project/{projectId}/teams")]
    public async Task<IActionResult> GetTeamsForInstructor(int projectId)
    {
        var response = await _teamService.GetProjectTeamsForInstructor(projectId);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }

    [RoleBasedAuthorization("Instructor")]
    [HttpDelete("{teamId}")]
    public async Task<IActionResult> DeleteClass(int teamId)
    {
        var response = await _teamService.DeleteTeam(teamId);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Message);
    }

    [RoleBasedAuthorization("Instructor")]
    [HttpPatch("{teamId}")]
    public async Task<IActionResult> UpdateTeamPartial(int teamId, [FromForm] PatchTeamDto dto)
    {
        var response = await _teamService.UpdateTeamPartial(teamId, dto);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Message);
    }
}