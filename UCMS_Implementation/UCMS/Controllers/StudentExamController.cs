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
    [HttpGet("template/{examId}")]
    public async Task<IActionResult> GetExerciseScoreTemplateFile(int examId)
    {
        var response = await _studentExamService.GetExamScoreTemplateFile(examId);

        if (!response.Success)
            return BadRequest(response);

        return File(response.Data.FileBytes, response.Data.ContentType, response.Data.FileName);
    }

    [RoleBasedAuthorization("Instructor")]
    [HttpPatch("{examId}/scores")]
    public async Task<IActionResult> UpdateExerciseSubmissionScores(int examId, IFormFile scoreFile)
    {
        var response = await _studentExamService.UpdateExamScores(examId, scoreFile);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}