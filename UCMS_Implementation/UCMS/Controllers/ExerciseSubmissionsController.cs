using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.DTOs.ExerciseSubmissionDto;
using UCMS.Services.ExerciseSubmissionService.Abstraction;

namespace UCMS.Controllers;
// student number
// sorted by name for student
// send enum to front
// report
// name of files
// filter by final
[ApiController]
[Route("api/[controller]")]
public class ExerciseSubmissionsController: ControllerBase
{
    private readonly IExerciseSubmissionService _exerciseSubmissionService;
    public ExerciseSubmissionsController(IExerciseSubmissionService exerciseSubmissionService)
    {
        _exerciseSubmissionService = exerciseSubmissionService;
    }

    [RoleBasedAuthorization("Student")]
    [HttpPost("{exerciseId}")]
    public async Task<IActionResult> CreateExerciseSubmission(int exerciseId, [FromForm] CreateExerciseSubmissionDto dto)
    {
        var response = await _exerciseSubmissionService.CreateExerciseSubmission(exerciseId, dto);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [RoleBasedAuthorization("Instructor")]
    [HttpGet("instructor/{exerciseSubmissionId}")]
    public async Task<IActionResult> GetExerciseSubmissionFileForInstructor(int exerciseSubmissionId)
    {
        var response = await _exerciseSubmissionService.GetExerciseSubmissionFileForInstructor(exerciseSubmissionId);
        
        if (!response.Success)
            return BadRequest(response);

        return File(response.Data.FileBytes, response.Data.ContentType, response.Data.FileName);
    }
    
    [RoleBasedAuthorization("Student")]
    [HttpGet("student/{exerciseSubmissionId}")]
    public async Task<IActionResult> GetExerciseSubmissionFileForStudent(int exerciseSubmissionId)
    {
        var response = await _exerciseSubmissionService.GetExerciseSubmissionFileForStudent(exerciseSubmissionId);
        
        if (!response.Success)
            return BadRequest(response);
        
        return File(response.Data.FileBytes, response.Data.ContentType, response.Data.FileName);
    }

    [RoleBasedAuthorization("Instructor")]
    [HttpGet("{exerciseId}")]
    public async Task<IActionResult> GetExerciseSubmissionFiles(int exerciseId)
    {
        var response = await _exerciseSubmissionService.GetExerciseSubmissionFiles(exerciseId);
        
        if (!response.Success)
            return BadRequest(response);

        return File(response.Data.FileBytes, response.Data.ContentType, response.Data.FileName);
    }
    
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("instructor")]
    public async Task<IActionResult> GetExerciseSubmissionsForInstructor([FromQuery] SortExerciseSubmissionsForInstructorDto dto)
    {
        var response = await _exerciseSubmissionService.GetExerciseSubmissionsForInstructor(dto);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [RoleBasedAuthorization("Student")]
    [HttpGet("student")]
    public async Task<IActionResult> GetExerciseSubmissionsForStudent([FromQuery] SortExerciseSubmissionsStudentDto dto)
    {
        var response = await _exerciseSubmissionService.GetExerciseSubmissionsForStudent(dto);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [RoleBasedAuthorization("Instructor")]
    [HttpGet("template/{exerciseId}")]
    public async Task<IActionResult> GetExerciseScoreTemplateFile(int exerciseId)
    {
        var response = await _exerciseSubmissionService.GetExerciseScoreTemplateFile(exerciseId);

        if (!response.Success)
            return BadRequest(response);

        return File(response.Data.FileBytes, response.Data.ContentType, response.Data.FileName);
    }

    [RoleBasedAuthorization("Student")]
    [HttpPatch("final/{exerciseSubmissionId}")]    
    public async Task<IActionResult> UpdateFinalExerciseSubmission(int exerciseSubmissionId)
    {
        var response = await _exerciseSubmissionService.UpdateFinalExerciseSubmission(exerciseSubmissionId);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [RoleBasedAuthorization("Instructor")]
    [HttpPatch("score/{exerciseSubmissionId}")]
    public async Task<IActionResult> UpdateExerciseSubmissionScore(int exerciseSubmissionId, [FromBody] UpdateExerciseSubmissionScoreDto dto)
    {
        var response = await _exerciseSubmissionService.UpdateExerciseSubmissionScore(exerciseSubmissionId, dto);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [RoleBasedAuthorization("Instructor")]
    [HttpPatch("{exerciseId}/scores")]
    public async Task<IActionResult> UpdateExerciseSubmissionScores(int exerciseId, IFormFile scoreFile)
    {
        var response = await _exerciseSubmissionService.UpdateExerciseSubmissionScores(exerciseId, scoreFile);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
    
}