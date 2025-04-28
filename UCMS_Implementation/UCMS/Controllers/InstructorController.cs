using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.DTOs.Instructor;
using UCMS.DTOs.Student;
using UCMS.Services.InstructorService;
using UCMS.Services.InstructorService.Abstraction;
using UCMS.Services.StudentService;
using UCMS.Services.StudentService.Abstraction;

namespace UCMS.Controllers
{
    [ApiController]
    [Route("api/instructors")]
    public class InstructorController: ControllerBase
    {
        private readonly IInstructorService _isntrutorService;

        public InstructorController(IInstructorService instructorService)
        {
            _isntrutorService = instructorService;
        }

        [HttpGet("profile/Specialized-info")]
        [Authorize]
        public async Task<ActionResult> GetInstructorById()
        {
            var response = await _isntrutorService.GetSpecializedInfo();

            if (response.Data == null) return NotFound(response.Message);

            return Ok(response);
        }

        [HttpGet("profile")]
        [RoleBasedAuthorization("Instructor")]
        public async Task<ActionResult> GetCurrentInstructorProfile()
        {
            var response = await _isntrutorService.GetCurrentInstructor();

            if (response.Data == null) return NotFound(response.Message);

            return Ok(response);
        }

        [HttpPut("profile/edit")]
        [RoleBasedAuthorization("Instructor")]
        public async Task<IActionResult> EditStudent([FromBody] EditInstructorDto editInstructorDto)
        {
            var result = await _isntrutorService.EditInstructor(editInstructorDto);

            if (!result.Success) return NotFound(result.Message);

            return Ok(result.Message);
        }
    }
}
