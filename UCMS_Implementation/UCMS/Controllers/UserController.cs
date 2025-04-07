using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UCMS.DTOs.User;
using UCMS.Models;
using UCMS.Services.UserService;


namespace UCMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<OutputUserDto>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OutputUserDto>> GetUserById(int id)
        {
            var response = await _userService.GetUserByIdAsync(id);

            if (response.Data == null) return NotFound(response.Message);

            return Ok(response);
        }
    }
}
