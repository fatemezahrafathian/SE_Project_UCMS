using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.DTOs.Student;
using UCMS.Models;
using UCMS.Services.StudentService.Abstraction;
using UCMS.Services.UserService;

namespace UCMS.Controllers
{
    [ApiController]
    [Route("api/students")] 
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllStudents()
        {
            return Ok(await _studentService.GetAllStudents());
        }

        [HttpGet("profile/specialized-info")]
        [Authorize]
        public async Task<ActionResult> GetSpecializedInfo()
        {
            var response = await _studentService.GetSpecializedInfo();

            if (response.Data == null) return NotFound(response.Message);

            return Ok(response);
        }

        [HttpGet("profile")]
        [RoleBasedAuthorization("Student")]
        public async Task<ActionResult> GetCurrentStudentProfile()
        {
            var response = await _studentService.GetCurrentStudent();

            if (response.Data == null) return NotFound(response.Message);

            return Ok(response);
        }

        [HttpPut("profile/edit")]
        [RoleBasedAuthorization("Student")]
        public async Task<IActionResult> EditStudent([FromBody] EditStudentDto editStudentDto)
        {
            var result = await _studentService.EditStudentAsync(editStudentDto);

            if (!result.Success) return NotFound(result.Message);

            return Ok(result.Message);
        }

    }
}
