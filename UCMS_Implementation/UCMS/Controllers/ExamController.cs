using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.DTOs.ExamDto;
using UCMS.Services.ExamService.Abstraction;

namespace UCMS.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ExamController:ControllerBase
{
    private readonly IExamService _examService;

    public ExamController(IExamService examService)
    {
        _examService = examService;
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpPost("")]
    public async Task<IActionResult> CreateExam(int classId,[FromForm] CreateExamDto dto)
    {
        var response = await _examService.CreateExamAsync(classId,dto);
    
        if (response.Success)
        {
            return CreatedAtAction(nameof(GetExamForInstructor), new { examId = response.Data.ExamId }, response);

        }
    
        return BadRequest(new {message = response.Message});
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("Instructor/{examId}")]
    public async Task<IActionResult> GetExamForInstructor(int examId)
    {
        var response = await _examService.GetExamByIdForInstructorAsync(examId);
        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpPatch("{examId}")]
    public async Task<IActionResult> UpdateExam(int examId, [FromForm] PatchExamDto dto)
    {
        var response = await _examService.UpdateExamAsync(examId, dto);

        if (response.Success)
            return Ok(response);

        return BadRequest(new { message = response.Message });
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpDelete("{examId}")]
    public async Task<IActionResult> DeleteExam(int examId)
    {
        var response = await _examService.DeleteExamAsync(examId);
        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Message);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("instructor/class/{classId}")]
    public async Task<IActionResult> GetExamsOfClassForInstructor(int classId)
    {
        var response = await _examService.GetExamsOfClassForInstructor(classId);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    
    [RoleBasedAuthorization("Student")]
    [HttpGet("Student/{examId}")]
    public async Task<IActionResult> GetExamForStudent(int examId)
    {
        var response = await _examService.GetExamByIdForStudentAsync(examId);
        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Student")]
    [HttpGet("Student/class/{classId}")]
    public async Task<IActionResult> GetExamsOfClassForStudent(int classId)
    {
        var response = await _examService.GetExamsOfClassForStudent(classId);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("/api/Instructor/exams")]
    public async Task<IActionResult> GetExamsForInstructor()
    {
        var response = await _examService.GetExamsForInstructor();

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Student")]
    [HttpGet("/api/Student/exams")]
    public async Task<IActionResult> GetExamsForStudent()
    {
        var response = await _examService.GetExamsForStudent();

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
}