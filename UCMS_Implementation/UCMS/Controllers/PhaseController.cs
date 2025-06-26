using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.DTOs.PhaseDto;
using UCMS.Services.PhaseService.Abstraction;

namespace UCMS.Controllers;
[Route("api/[controller]")]
[ApiController]
public class PhaseController:ControllerBase
{
    private readonly IPhaseService _phaseService;

    public PhaseController(IPhaseService phaseService)
    {
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
    public async Task<IActionResult> UpdatePhase(int phaseId, [FromForm] PatchPhaseDto dto)
    {
        var response = await _phaseService.UpdatePhaseAsync(phaseId, dto);

        if (response.Success)
            return Ok(response);

        return BadRequest(new { message = response.Message });
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpDelete("{phaseId}")]
    public async Task<IActionResult> DeletePhase(int phaseId)
    {
        var response = await _phaseService.DeletePhaseAsync(phaseId);
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
        var response = await _phaseService.HandleDownloadPhaseFileForInstructorAsync(phaseId);
        if (!response.Success)
            return NotFound(response.Message);
    
        return File(response.Data.FileBytes, response.Data.ContentType, response.Data.FileName);
    }
    
    [RoleBasedAuthorization("Student")]
    [HttpGet("Student/{phaseId}")]
    public async Task<IActionResult> GetPhaseForStudent(int phaseId)
    {
        var response = await _phaseService.GetPhaseByIdForStudentAsync(phaseId);
        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Student")]
    [HttpGet("Student")]
    public async Task<IActionResult> GetPhasesForStudent(int projectId)
    {
        var response = await _phaseService.GetPhasesForStudent(projectId);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Student")]
    [HttpGet("{phaseId}/downloadForStudent")]
    public async Task<IActionResult> DownloadPhaseFileForStudent(int phaseId)
    {
        var response = await _phaseService.HandleDownloadPhaseFileForStudentAsync(phaseId);
        if (!response.Success)
            return NotFound(response.Message);
    
        return File(response.Data.FileBytes, response.Data.ContentType, response.Data.FileName);
    }
    
}