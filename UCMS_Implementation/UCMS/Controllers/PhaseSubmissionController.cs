using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.DTOs.TeamPhaseDto;
using UCMS.Services.TeamPhaseSrvice;

namespace UCMS.Controllers;
// create submission (student)
// download submission file (student, instructor)
// download submission files (instructor)
// get team submissions (student)
// sort submissions by date (student)
// get final submisstions (instructor)
// sort submissions by date or team name (instructor)
// delete submission (student)

// download template file (instructor)
// submit scores (student)
// submit score (student)
// edit scores (instructor)

[ApiController]
[Route("api/[controller]")]
public class PhaseSubmissionController: ControllerBase
{
    private readonly IPhaseSubmissionService _phaseSubmissionService;

    public PhaseSubmissionController(IPhaseSubmissionService phaseSubmissionService)
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
    [HttpGet("instructor/{submissionId}")]
    public async Task<IActionResult> GetPhaseSubmissionFileForInstructor(int submissionId)
    {
        var response = await _phaseSubmissionService.GetPhaseSubmissionFileForInstructor(submissionId);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [RoleBasedAuthorization("Student")]
    [HttpGet("student/{submissionId}")]
    public async Task<IActionResult> GetPhaseSubmissionFileForStudent(int submissionId)
    {
        var response = await _phaseSubmissionService.GetPhaseSubmissionFileForStudent(submissionId);
        
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
    public async Task<IActionResult> GetPhaseSubmissionsForInstructor([FromQuery] SortPhaseSubmissionsForInsrtuctorDto dto)
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
    [HttpPut("{submissionId}")]    
    public async Task<IActionResult> UpdateFinalSubmission(int submissionId)
    {
        var response = await _phaseSubmissionService.UpdateFinalSubmission(submissionId);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}