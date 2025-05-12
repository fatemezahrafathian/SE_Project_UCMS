using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.Data;
using UCMS.DTOs.ClassDto;
using UCMS.Services.ClassService.Abstraction;

namespace UCMS.Controllers;

// determine nullability of dtos properties
// handle invalid role id
// captch
// cloud
// confirmation link
// email service
[Route("api/[controller]")]
[ApiController]
public class ClassesController : ControllerBase
{
    private readonly IClassService _classService;
    private readonly DataContext _context;

    public ClassesController(IClassService classService, DataContext context)
    {
        _classService = classService;
        _context = context;
    }


    [RoleBasedAuthorization("Instructor")]
    [HttpPost("")]
    public async Task<IActionResult> CreateClass([FromForm] CreateClassDto dto)
    {
        var response = await _classService.CreateClass(dto);

        if (response.Success)
        {
            return CreatedAtAction(nameof(GetClassForInstructor), new {classId = response.Data.Id}, response);
        }

        return BadRequest(new {message = response.Message});
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
    
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("instructor")]
    public async Task<IActionResult> GetClassesForInstructor([FromQuery] PaginatedFilterClassForInstructorDto dto)
    {
        var response = await _classService.GetClassesForInstructor(dto);

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

    [RoleBasedAuthorization("Instructor")]
    [HttpPatch("{classId}")]
    public async Task<IActionResult> UpdateClassPartial(int classId, [FromForm] PatchClassDto dto)
    {
        var response = await _classService.UpdateClassPartial(classId, dto);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Message);
    }

    // [HttpPost("test{userId}")]
    // public async Task<IActionResult> Createins(int userId)
    // {
    //     var testInstructor = new Instructor
    //     {
    //         UserId = userId,
    //         EmployeeCode = "EMP-001",
    //         Department = "مهندسی نرم‌افزار",
    //         CreatedAt = DateTime.UtcNow,
    //         UpdatedAt = DateTime.UtcNow
    //     };
    //     _context.Instructors.Add(testInstructor);
    //     await _context.SaveChangesAsync();
    //     return Ok(1);
    // }
    //
    // [HttpGet("test1{userId}")]
    // public async Task<IActionResult> Createstu(int userId)
    // {
    //     var testStudent = new Student
    //     {
    //         UserId = userId,
    //         StudentNumber = "STD123456",
    //         Major = "Computer Science",
    //         EnrollmentYear = 2023
    //     };        
    //     _context.Students.Add(testStudent);
    //     await _context.SaveChangesAsync();
    //     return Ok(1);
    // }
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