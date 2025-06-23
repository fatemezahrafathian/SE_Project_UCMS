using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.Services.StudentExamService.Abstraction;

namespace UCMS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StudentExamController: ControllerBase
{
    private readonly IStudentExamService _studentExamService;

    public StudentExamController(IStudentExamService studentExamService)
    {
        _studentExamService = studentExamService;
    }

    [RoleBasedAuthorization("Instructor")]
    [HttpGet("template/{exerciseId}")]
    public async Task<IActionResult> GetExerciseScoreTemplateFile(int exerciseId)
    {
        var response = await _studentExamService.GetExamScoreTemplateFile(exerciseId);

        if (!response.Success)
            return BadRequest(response);

        return File(response.Data.FileBytes, response.Data.ContentType, response.Data.FileName);
    }

    [RoleBasedAuthorization("Instructor")]
    [HttpPatch("{examId}/scores")]
    public async Task<IActionResult> UpdateExerciseSubmissionScores(int examId, [FromForm] IFormFile scoreFile)
    {
        var response = await _studentExamService.UpdateExamScores(examId, scoreFile);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}