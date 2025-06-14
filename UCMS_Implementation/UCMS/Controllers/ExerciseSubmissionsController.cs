using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.DTOs.ExerciseSubmissionDto;
using UCMS.Services.ExerciseSubmissionService.Abstraction;

namespace UCMS.Controllers;

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

        return Ok(response);
    }

    [RoleBasedAuthorization("Student")]
    [HttpGet("student/{exerciseSubmissionId}")]
    public async Task<IActionResult> GetExerciseSubmissionFileForStudent(int exerciseSubmissionId)
    {
        var response = await _exerciseSubmissionService.GetExerciseSubmissionFileForStudent(exerciseSubmissionId);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [RoleBasedAuthorization("Instructor")]
    [HttpGet("{exerciseSubmissionId}")]
    public async Task<IActionResult> GetExerciseSubmissionFiles(int exerciseSubmissionId)
    {
        var response = await _exerciseSubmissionService.GetExerciseSubmissionFiles(exerciseSubmissionId);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
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

    [RoleBasedAuthorization("Student")]
    [HttpPut("{exerciseSubmissionId}")]    
    public async Task<IActionResult> UpdateFinalSubmission(int exerciseSubmissionId)
    {
        var response = await _exerciseSubmissionService.UpdateFinalSubmission(exerciseSubmissionId);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

}