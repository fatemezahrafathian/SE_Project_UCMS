using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }
    }
}
