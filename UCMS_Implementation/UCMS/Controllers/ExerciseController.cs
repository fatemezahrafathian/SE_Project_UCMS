using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.Data;
using UCMS.DTOs.ExerciseDto;
using UCMS.Services.ExerciseService.Abstraction;

namespace UCMS.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ExerciseController:ControllerBase
{
    private readonly DataContext _context;
    private readonly IExerciseService _ExerciseService;

    public ExerciseController(DataContext context, IExerciseService ExerciseService)
    {
        _context = context;
        _ExerciseService = ExerciseService;
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpPost("")]
    public async Task<IActionResult> CreateExercise(int classId,[FromForm] CreateExerciseDto dto)
    {
        var response = await _ExerciseService.CreateExerciseAsync(classId,dto);
    
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
        var response = await _ExerciseService.GetExerciseByIdForInstructorAsync(exerciseId);
        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpPatch("{exerciseId}")]
    public async Task<IActionResult> UpdateExercise(int classId, int exerciseId, [FromForm] PatchExerciseDto dto)
    {
        var response = await _ExerciseService.UpdateExerciseAsync(exerciseId, dto);

        if (response.Success)
            return Ok(response);

        return BadRequest(new { message = response.Message });
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpDelete("{exerciseId}")]
    public async Task<IActionResult> DeleteExercise(int classId, int exerciseId)
    {
        var response = await _ExerciseService.DeleteExerciseAsync(exerciseId);
        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Message);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("instructor")]
    public async Task<IActionResult> GetExercisesForInstructor(int classId)
    {
        var response = await _ExerciseService.GetExercisesForInstructor(classId);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("{exerciseId}/downloadForInstructor")]
    public async Task<IActionResult> DownloadExerciseFileForInstructor(int exerciseId)
    {
        var response = await _ExerciseService.HandleDownloadExerciseFileForInstructorAsync(exerciseId);
        if (!response.Success)
            return NotFound(response.Message);
    
        return File(response.Data.FileBytes, response.Data.ContentType, response.Data.FileName);
    }
    
    [RoleBasedAuthorization("Student")]
    [HttpGet("Student/{exerciseId}")]
    public async Task<IActionResult> GetExerciseForStudent(int exerciseId)
    {
        var response = await _ExerciseService.GetExerciseByIdForStudentAsync(exerciseId);
        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Student")]
    [HttpGet("Student")]
    public async Task<IActionResult> GetExercisesForStudent(int classId)
    {
        var response = await _ExerciseService.GetExercisesForStudent(classId);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Student")]
    [HttpGet("{exerciseId}/downloadForStudent")]
    public async Task<IActionResult> DownloadExerciseFileForStudent(int exerciseId)
    {
        var response = await _ExerciseService.HandleDownloadExerciseFileForStudentAsync(exerciseId);
        if (!response.Success)
            return NotFound(response.Message);
    
        return File(response.Data.FileBytes, response.Data.ContentType, response.Data.FileName);
    }
}