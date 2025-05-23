using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.Data;
using UCMS.DTOs.PhaseDto;
using UCMS.DTOs.ProjectDto;
using UCMS.Services.ClassService.Abstraction;
using UCMS.Services.PhaseService.Abstraction;

namespace UCMS.Controllers;
[Route("api/[controller]")]
[ApiController]
public class PhaseController:ControllerBase
{
    private readonly DataContext _context;
    private readonly IPhaseService _phaseService;

    public PhaseController(DataContext context, IPhaseService phaseService)
    {
        _context = context;
        _phaseService = phaseService;
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpPost("")]
    public async Task<IActionResult> CreatePhase(int projectId,[FromForm] CreatePhaseDto dto)
    {
        var response = await _phaseService.CreatePhaseAsync(projectId,dto);
    
        if (response.Success)
        {
            return CreatedAtAction(nameof(GetPhaseForInstructor), new { phaseId = response.Data.phaseId }, response);

        }
    
        return BadRequest(new {message = response.Message});
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("Instructor/{phaseId}")]
    public async Task<IActionResult> GetPhaseForInstructor(int phaseId)
    {
        var response = await _phaseService.GetPhaseByIdForInstructorAsync(phaseId);
        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpPatch("{phaseId}")]
    public async Task<IActionResult> UpdatePhase(int projectId, int phaseId, [FromForm] PatchPhaseDto dto)
    {
        var response = await _phaseService.UpdatePhaseAsync(projectId, phaseId, dto);

        if (response.Success)
            return Ok(response);

        return BadRequest(new { message = response.Message });
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpDelete("{phaseId}")]
    public async Task<IActionResult> DeletePhase(int projectId, int phaseId)
    {
        var response = await _phaseService.DeletePhaseAsync(projectId, phaseId);
        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Message);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("instructor")]
    public async Task<IActionResult> GetPhasesForInstructor(int projectId)
    {
        var response = await _phaseService.GetPhasesForInstructor(projectId);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("{phaseId}/downloadForInstructor")]
    public async Task<IActionResult> DownloadPhaseFileForInstructor(int phaseId)
    {
        var response = await _phaseService.HandleDownloadPhaseFileAsync(phaseId);
        if (!response.Success)
            return NotFound(response.Message);
    
        return File(response.Data.FileBytes, response.Data.ContentType, response.Data.FileName);
    }

    
}