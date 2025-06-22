using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.DTOs.ExerciseSubmissionDto;

namespace UCMS.Controllers;
//  dictionary
// 
[ApiController]
[Route("api/[controller]")]
public class ScoreReportsController: ControllerBase
{
    // private readonly IScoreReportService _scoreReportService;
    // public ScoreReportsController(IScoreReportService scoreReportService)
    // {
    //     _scoreReportService = scoreReportService;
    // }
    //
    // [RoleBasedAuthorization("Student")]
    // [HttpPost("{exerciseId}")]
    // public async Task<IActionResult> CreateExerciseSubmission(int exerciseId, [FromForm] CreateExerciseSubmissionDto dto)
    // {
    //     var response = await _scoreReportService.CreateExerciseSubmission(exerciseId, dto);
    //     
    //     if (!response.Success)
    //         return BadRequest(response);
    //
    //     return Ok(response);
    // }

}