using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.DTOs.ExerciseDto;
using UCMS.Services.ExerciseService.Abstraction;

namespace UCMS.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ExerciseController:ControllerBase
{
    private readonly IExerciseService _exerciseService;

    public ExerciseController(IExerciseService exerciseService)
    {
        _exerciseService = exerciseService;
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpPost("")]
    public async Task<IActionResult> CreateExercise(int classId,[FromForm] CreateExerciseDto dto)
    {
        var response = await _exerciseService.CreateExerciseAsync(classId,dto);
    
        if (response.Success)
        {
            return CreatedAtAction(nameof(GetExerciseForInstructor), new { exerciseId = response.Data.exerciseId }, response);

        }
    
        return BadRequest(new {message = response.Message});
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("Instructor/{exerciseId}")]
    public async Task<IActionResult> GetExerciseForInstructor(int exerciseId)
    {
        var response = await _exerciseService.GetExerciseByIdForInstructorAsync(exerciseId);
        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpPatch("{exerciseId}")]
    public async Task<IActionResult> UpdateExercise(int exerciseId, [FromForm] PatchExerciseDto dto)
    {
        var response = await _exerciseService.UpdateExerciseAsync(exerciseId, dto);

        if (response.Success)
            return Ok(response);

        return BadRequest(new { message = response.Message });
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpDelete("{exerciseId}")]
    public async Task<IActionResult> DeleteExercise( int exerciseId)
    {
        var response = await _exerciseService.DeleteExerciseAsync(exerciseId);
        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Message);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("Instructor/class/{classId}")]
    public async Task<IActionResult> GetExercisesOfClassForInstructor(int classId)
    {
        var response = await _exerciseService.GetExercisesOfClassForInstructor(classId);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("{exerciseId}/downloadForInstructor")]
    public async Task<IActionResult> DownloadExerciseFileForInstructor(int exerciseId)
    {
        var response = await _exerciseService.HandleDownloadExerciseFileForInstructorAsync(exerciseId);
        if (!response.Success)
            return NotFound(response.Message);
    
        return File(response.Data.FileBytes, response.Data.ContentType, response.Data.FileName);
    }
    
    [RoleBasedAuthorization("Student")]
    [HttpGet("Student/{exerciseId}")]
    public async Task<IActionResult> GetExerciseForStudent(int exerciseId)
    {
        var response = await _exerciseService.GetExerciseByIdForStudentAsync(exerciseId);
        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Student")]
    [HttpGet("Student/class/{classId}")]
    public async Task<IActionResult> GetExercisesOfClassForStudent(int classId)
    {
        var response = await _exerciseService.GetExercisesOfClassForStudent(classId);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Student")]
    [HttpGet("{exerciseId}/downloadForStudent")]
    public async Task<IActionResult> DownloadExerciseFileForStudent(int exerciseId)
    {
        var response = await _exerciseService.HandleDownloadExerciseFileForStudentAsync(exerciseId);
        if (!response.Success)
            return NotFound(response.Message);
    
        return File(response.Data.FileBytes, response.Data.ContentType, response.Data.FileName);
    }
    [RoleBasedAuthorization("Student")]
    [HttpGet("/api/Exercises/Student")]
    public async Task<IActionResult> GetExercisesForStudent()
    {
        var response = await _exerciseService.GetExercisesForStudent();

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("/api/Exercises/instructor")]
    public async Task<IActionResult> GetExercisesForInstructor()
    {
        var response = await _exerciseService.GetExercisesForInstructor();

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
}