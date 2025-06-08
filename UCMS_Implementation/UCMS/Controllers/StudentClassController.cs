using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.DTOs.ClassDto;
using UCMS.Services.ClassService.Abstraction;

namespace UCMS.Controllers;
[Route("api/[controller]")]
[ApiController]
public class StudentClassController: ControllerBase
{

    private readonly IStudentClassService _studentClassService;
    public StudentClassController(IStudentClassService studentClassService)
    {
        _studentClassService = studentClassService;
    }
    
    [RoleBasedAuthorization("Student")]
    [HttpPost("Join")]
    public async Task<IActionResult> JoinClass([FromBody] JoinClassRequestDto request)
    {
        var response = await _studentClassService.JoinClassAsync(request);
        if (response.Success)
            return Ok(response);
        return BadRequest(response.Message);
    }
    [RoleBasedAuthorization("Student")]
    [HttpDelete("{classId}/Leave")]
    public async Task<IActionResult> LeaveClass([FromBody] LeaveClassRequestDto request)
    {
        var response = await _studentClassService.LeaveClassAsync(request.ClassId);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Message);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpDelete("{classId}/Students/remove")]
    public async Task<IActionResult> RemoveStudentFromClass([FromBody] RemoveStudentFromClassDto request)
    {
        var response = await _studentClassService.RemoveStudentFromClassAsync(request.ClassId, request.StudentId);
        
        if (!response.Success)
            return NotFound(response.Message);
        return Ok(response.Message);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("Instructor/{classId}/Students")]
    public async Task<IActionResult> GetStudentsOfClassByInstructor(int classId)
    {
        var response = await _studentClassService.GetStudentsOfClassByInstructorAsync(classId);
        if (!response.Success)
            return NotFound(response.Message);
        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Student")]
    [HttpGet("Student/{classId}/Students")]
    public async Task<IActionResult> GetStudentsOfClassByStudent(int classId)
    {
        var response = await _studentClassService.GetStudentsOfClassByStudentAsync(classId);
        if (!response.Success)
            return NotFound(response.Message);
        return Ok(response.Data);
    }

    [RoleBasedAuthorization("Student")]
    [HttpGet("Student/{classId}")]
    public async Task<IActionResult> GetClassForStudent(int classId)
    {
        var response = await _studentClassService.GetClassForStudent(classId);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Student")]
    [HttpGet("Student/classes")]
    public async Task<IActionResult> GetClassesForStudent([FromQuery] PaginatedFilterClassForStudentDto dto)
    {
        var response = await _studentClassService.GetClassesForStudent(dto);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
}