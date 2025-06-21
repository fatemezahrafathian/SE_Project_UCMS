using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.DTOs.PhaseSubmissionDto;
using UCMS.Services.TeamPhaseSrvice;

namespace UCMS.Controllers;
// add api to get studentPhaseSubmissions
// return score in phase submissions for student
// check submission existence in score studentTeamPhase
// merge excels in one service
// just add names who have been in a team for the phase to the template file
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

        return Ok(response);
    }

    [RoleBasedAuthorization("Instructor")]
    [HttpGet("instructor/{phaseSubmissionId}")]
    public async Task<IActionResult> GetPhaseSubmissionFileForInstructor(int phaseSubmissionId)
    {
        var response = await _phaseSubmissionService.GetPhaseSubmissionFileForInstructor(phaseSubmissionId);
        
        if (!response.Success)
            return BadRequest(response);

        return File(response.Data.FileBytes, response.Data.ContentType, response.Data.FileName);
    }

    [RoleBasedAuthorization("Student")]
    [HttpGet("student/{phaseSubmissionId}")]
    public async Task<IActionResult> GetPhaseSubmissionFileForStudent(int phaseSubmissionId)
    {
        var response = await _phaseSubmissionService.GetPhaseSubmissionFileForStudent(phaseSubmissionId);
        
        if (!response.Success)
            return BadRequest(response);

        return File(response.Data.FileBytes, response.Data.ContentType, response.Data.FileName);
    }

    [RoleBasedAuthorization("Instructor")]
    [HttpGet("{phaseId}")]
    public async Task<IActionResult> GetPhaseSubmissionFiles(int phaseId)
    {
        var response = await _phaseSubmissionService.GetPhaseSubmissionFiles(phaseId);
        
        if (!response.Success)
            return BadRequest(response);

        return File(response.Data.FileBytes, response.Data.ContentType, response.Data.FileName);
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
    
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("{phaseId}/{teamId}/members")]
    public async Task<IActionResult> GetTeamPhaseMembers(int phaseId, int teamId)
    {
        var response = await _phaseSubmissionService.GetTeamPhaseMembers(phaseId, teamId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [RoleBasedAuthorization("Instructor")]
    [HttpGet("template/{phaseId}")]
    public async Task<IActionResult> GetPhaseScoreTemplateFile(int phaseId)
    {
        var response = await _phaseSubmissionService.GetPhaseScoreTemplateFile(phaseId);

        if (!response.Success)
            return BadRequest(response);

        return File(response.Data.FileBytes, response.Data.ContentType, response.Data.FileName);
    }

    [RoleBasedAuthorization("Student")]
    [HttpPatch("final/{phaseSubmissionId}")]    
    public async Task<IActionResult> UpdateFinalSubmission(int phaseSubmissionId)
    {
        var response = await _phaseSubmissionService.UpdateFinalPhaseSubmission(phaseSubmissionId);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
    
    [RoleBasedAuthorization("Instructor")]
    [HttpPatch("score/{studentTeamPhaseId}")]
    public async Task<IActionResult> UpdatePhaseSubmissionScore(int studentTeamPhaseId, [FromBody] UpdatePhaseSubmissionScoreDto dto)
    {
        var response = await _phaseSubmissionService.UpdatePhaseSubmissionScore(studentTeamPhaseId, dto);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [RoleBasedAuthorization("Instructor")]
    [HttpPatch("{phaseId}/scores")]
    public async Task<IActionResult> UpdatePhaseSubmissionScores(int phaseId, IFormFile scoreFile)
    {
        var response = await _phaseSubmissionService.UpdatePhaseSubmissionScores(phaseId, scoreFile);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
    
}