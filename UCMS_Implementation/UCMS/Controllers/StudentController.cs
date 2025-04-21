using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UCMS.DTOs.Student;
using UCMS.Models;
using UCMS.Services.StudentService.Abstraction;

namespace UCMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Ensure only authenticated users can access this controller  
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpPut("edit")]
        public async Task<IActionResult> EditStudent([FromBody] EditStudentDto editStudentDto)
        {
            

            var result = await _studentService.EditStudentAsync(user.Id, editStudentDto);
            if (!result) return NotFound("Student not found");

            return Ok();
        }
    }

}
