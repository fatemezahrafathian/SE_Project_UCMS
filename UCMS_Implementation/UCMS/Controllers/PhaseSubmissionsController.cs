using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.DTOs.TeamPhaseDto;
using UCMS.Services.TeamPhaseSrvice;

namespace UCMS.Controllers;
// download template file (instructor)
// submit scores (student)
// submit score (student)
// edit scores (instructor)

[ApiController]
[Route("api/[controller]")]
public class PhaseSubmissionsController: ControllerBase
{
    private readonly IPhaseSubmissionService _phaseSubmissionService;

    public PhaseSubmissionsController(IPhaseSubmissionService phaseSubmissionService)
    {
        _phaseSubmissionService = phaseSubmissionService;
    }

    [RoleBasedAuthorization("Student")]
    [HttpPost("{phaseId}")]
    public async Task<IActionResult> CreatePhaseSubmission(int phaseId, [FromForm] CreatePhaseSubmissionDto dto)
    {
        var response = await _phaseSubmissionService.CreatePhaseSubmission(phaseId, dto);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);  // message
    }

    [RoleBasedAuthorization("Instructor")]
    [HttpGet("instructor/{phaseSubmissionId}")]
    public async Task<IActionResult> GetPhaseSubmissionFileForInstructor(int phaseSubmissionId)
    {
        var response = await _phaseSubmissionService.GetPhaseSubmissionFileForInstructor(phaseSubmissionId);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [RoleBasedAuthorization("Student")]
    [HttpGet("student/{phaseSubmissionId}")]
    public async Task<IActionResult> GetPhaseSubmissionFileForStudent(int phaseSubmissionId)
    {
        var response = await _phaseSubmissionService.GetPhaseSubmissionFileForStudent(phaseSubmissionId);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [RoleBasedAuthorization("Instructor")]
    [HttpGet("{phaseId}")]
    public async Task<IActionResult> GetPhaseSubmissionFiles(int phaseId)
    {
        var response = await _phaseSubmissionService.GetPhaseSubmissionFiles(phaseId);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
    
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("instructor")]
    public async Task<IActionResult> GetPhaseSubmissionsForInstructor([FromQuery] SortPhaseSubmissionsForInstructorDto dto)
    {
        var response = await _phaseSubmissionService.GetPhaseSubmissionsForInstructor(dto);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [RoleBasedAuthorization("Student")]
    [HttpGet("student")]
    public async Task<IActionResult> GetPhaseSubmissionsForStudent([FromQuery] SortPhaseSubmissionsStudentDto dto)
    {
        var response = await _phaseSubmissionService.GetPhaseSubmissionsForStudent(dto);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [RoleBasedAuthorization("Student")]
    [HttpPut("{phaseSubmissionId}")]    
    public async Task<IActionResult> UpdateFinalSubmission(int phaseSubmissionId)
    {
        var response = await _phaseSubmissionService.UpdateFinalSubmission(phaseSubmissionId);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}