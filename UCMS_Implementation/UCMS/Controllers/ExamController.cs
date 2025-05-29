using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.Data;
using UCMS.DTOs.ExamDto;
using UCMS.Services.ExamService.Abstraction;

namespace UCMS.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ExamController:ControllerBase
{
    private readonly DataContext _context;
    private readonly IExamService _ExamService;

    public ExamController(DataContext context, IExamService ExamService)
    {
        _context = context;
        _ExamService = ExamService;
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpPost("")]
    public async Task<IActionResult> CreateExam(int classId,[FromForm] CreateExamDto dto)
    {
        var response = await _ExamService.CreateExamAsync(classId,dto);
    
        if (response.Success)
        {
            return CreatedAtAction(nameof(GetExamForInstructor), new { examId = response.Data.examId }, response);

        }
    
        return BadRequest(new {message = response.Message});
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("Instructor/{examId}")]
    public async Task<IActionResult> GetExamForInstructor(int examId)
    {
        var response = await _ExamService.GetExamByIdForInstructorAsync(examId);
        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpPatch("{examId}")]
    public async Task<IActionResult> UpdateExam(int classId,int examId, [FromForm] PatchExamDto dto)
    {
        var response = await _ExamService.UpdateExamAsync(examId, dto);

        if (response.Success)
            return Ok(response);

        return BadRequest(new { message = response.Message });
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpDelete("{examId}")]
    public async Task<IActionResult> DeleteExam(int classId, int examId)
    {
        var response = await _ExamService.DeleteExamAsync(examId);
        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Message);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("instructor")]
    public async Task<IActionResult> GetExamsForInstructor(int classId)
    {
        var response = await _ExamService.GetExamsForInstructor(classId);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    
    [RoleBasedAuthorization("Student")]
    [HttpGet("Student/{examId}")]
    public async Task<IActionResult> GetExamForStudent(int examId)
    {
        var response = await _ExamService.GetExamByIdForStudentAsync(examId);
        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Student")]
    [HttpGet("Student")]
    public async Task<IActionResult> GetExamsForStudent(int classId)
    {
        var response = await _ExamService.GetExamsForStudent(classId);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
}