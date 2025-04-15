using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.Data;
using UCMS.DTOs.ClassDto;
using UCMS.Models;
using UCMS.Services.ClassService.Abstraction;

namespace UCMS.Controllers;
// determine nullability of dtos properties
// role back if not completed
// handle invalid role id
// uniq class name or not
// check start time to be less than end time
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
    
    [HttpPost("test1{userId}")]
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
    
}

// {
// "title": "Programming Class",
// "description": "Introduction to C# programming.",
// "startDate": "2025-04-11T09:00:00.000Z",
// "endDate": "2025-04-11T11:00:00.000Z",
// "identifierType": 1,
// "schedules": [
// {
//     "dayOfWeek": 1,
//     "startTime": "09:00:00",
//     "endTime": "11:00:00"
// }
// ]
// }

//
// {
// "id": 4,
// "title": "Advanced Software Engineering",
// "description": "A comprehensive course on advanced software engineering topics.",
// "startDate": "2025-04-11T09:00:00.000Z",
// "endDate": "2025-06-25T09:00:00.000Z",
// "schedules": [
// {
//     "dayOfWeek": 1,
//     "startTime": "10:00:00",
//     "endTime": "12:00:00"
// },
// {
// "dayOfWeek": 4,
// "startTime": "13:00:00",
// "endTime": "15:00:00"
// }
// ]
// }

