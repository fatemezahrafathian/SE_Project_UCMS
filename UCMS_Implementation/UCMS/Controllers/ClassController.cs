using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.Data;
using UCMS.DTOs.ClassDto;
using UCMS.Models;
using UCMS.Services.ClassService.Abstraction;

namespace UCMS.Controllers;
// determine nullability of dtos properties
// handle invalid role id
// captch
// cloud
// confirmation link
// clean and reuse checks in services
[Route("api/classes")]
[ApiController]
public class ClassController: ControllerBase
{
    private readonly IClassService _classService;
    private readonly DataContext _context;

    public ClassController(IClassService classService, DataContext context)
    {
        _classService = classService;
        _context = context;
    }

    
    [RoleBasedAuthorization("Instructor")] // which comes first
    [HttpPost("")]
    public async Task<IActionResult> CreateClass([FromForm] CreateClassDto dto)
    {
        var response = await _classService.CreateClass(dto);

        if (response.Success)
        {
            return CreatedAtAction(nameof(GetClassForInstructor), new { classId = response.Data.Id }, response);
        }
        
        return BadRequest(new { message = response.Message });
    }


    [RoleBasedAuthorization("Instructor")]
    [HttpGet("instructor/{classId}")]
    public async Task<IActionResult> GetClassForInstructor(int classId)
    {
        var response = await _classService.GetClassForInstructor(classId);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }

    [RoleBasedAuthorization("Student")]
    [HttpGet("student/{classId}")]
    public async Task<IActionResult> GetClassForStudent(int classId)
    {
        var response = await _classService.GetClassForStudent(classId);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }

    [RoleBasedAuthorization("Instructor")]
    [HttpGet("instructor")]
    public async Task<IActionResult> FilteredClassesOfInstructor(PaginatedFilterClassForInstructorDto dto)
    {
        var response = await _classService.FilterClassesOfInstructor(dto);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    
    [RoleBasedAuthorization("Instructor")]
    [HttpDelete("{classId}")]
    public async Task<IActionResult> DeleteClass(int classId)
    {
        var response = await _classService.DeleteClass(classId);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Message);
    }
    
    [HttpPost("test{userId}")]
    public async Task<IActionResult> Createins(int userId)
    {
        var testInstructor = new Instructor
        {
            UserId = userId,
            EmployeeCode = "EMP-001",
            Department = "مهندسی نرم‌افزار",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Instructors.Add(testInstructor);
        await _context.SaveChangesAsync();
        return Ok(1);
    }
    
    [HttpGet("test1{userId}")]
    public async Task<IActionResult> Createstu(int userId)
    {
        var testStudent = new Student
        {
            UserId = userId,
            StudentNumber = "STD123456",
            Major = "Computer Science",
            EnrollmentYear = 2023
        };        
        _context.Students.Add(testStudent);
        await _context.SaveChangesAsync();
        return Ok(1);
    }

    [RoleBasedAuthorization("Instructor")]
    [HttpPatch("{classId}")]
    public async Task<IActionResult> PartialUpdateClass(int classId, [FromForm] PatchClassDto dto)
    {
        var response = await _classService.PartialUpdateClass(classId, dto);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Message);
    }
    [RoleBasedAuthorization("Student")] // which comes first
    [HttpPost("join")]
    public async Task<IActionResult> JoinClass([FromBody] JoinClassRequestDto request)
    {
        var response = await _classService.JoinClassAsync(request);
        if (response.Success)
            return Ok(response);
        return BadRequest(response.Message);
    }
    [RoleBasedAuthorization("Student")]
    [HttpDelete("left/{classId}")]
    public async Task<IActionResult> LeaveClass([FromBody] LeaveClassRequestDto request)
    {
        var response = await _classService.LeaveClassAsync(request.ClassId);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Message);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpDelete("/{classId}/removeStudent")]
    public async Task<IActionResult> RemoveStudentFromClass([FromBody] RemoveStudentFromClassDto request)
    {
        var response = await _classService.RemoveStudentFromClassAsync(request.ClassId, request.StudentId);
        
        if (!response.Success)
            return NotFound(response.Message);
        return Ok(response.Message);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("{classId}/students")]
    public async Task<IActionResult> GetStudentsOfClassByInstructor(int classId)
    {
        var response = await _classService.GetStudentsOfClassByInstructorAsync(classId);
        if (!response.Success)
            return NotFound(response.Message);
        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Student")]
    [HttpGet("classStudent/{classId}/students")]
    public async Task<IActionResult> GetStudentsOfClassByStudent(int classId)
    {
        var response = await _classService.GetStudentsOfClassByStudentAsync(classId);
        if (!response.Success)
            return NotFound(response.Message);
        return Ok(response.Data);
    }

    
}

// curl -X POST https://localhost:44389/api/classes ^
//     -H "accept: */*" ^
//     -H "Content-Type: multipart/form-data" ^
//     -F "Title=Algorithms" ^
//     -F "Description=First Year Class" ^
//     -F "StartDate=2025-04-25" ^
//     -F "EndDate=2025-07-30" ^
//     -F "ProfileImage=@C:/Users/Hana.N/Downloads/test.png" ^
//     -F "Schedules[0].DayOfWeek=1" ^
//     -F "Schedules[0].StartTime=08:00:00" ^
//     -F "Schedules[0].EndTime=10:00:00" ^
//     -F "Schedules[1].DayOfWeek=3" ^
//     -F "Schedules[1].StartTime=14:00:00" ^
//     -F "Schedules[1].EndTime=16:00:00" -k
