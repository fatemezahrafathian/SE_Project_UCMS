using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UCMS.Attributes;
using UCMS.DTOs.User;
using UCMS.Models;
using UCMS.Resources;
using UCMS.Services.UserService;


namespace UCMS.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult> GetUserById(int id)
        {
            var response = await _userService.GetUserByIdAsync(id);

            if (response.Data == null) return NotFound(response.Message);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        [RoleBasedAuthorization("Instructor")]
        public async Task<ActionResult> deleteUserById(int id)
        {
            var response = await _userService.DeleteUserAsync(id);

            if (response.Data == false) return NotFound(response.Message);

            return Ok(response);
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult> GetCurrentUserProfile()
        {
            var response = _userService.GetCurrentUser();

            if (response.Data == null) return NotFound(response.Message);

            return Ok(response);
        }

        [HttpPut("profile/edit")]
        public async Task<ActionResult> EditProfile([FromBody] EditUserDto editUserDto)
        {
            var response = await _userService.EditUser(editUserDto);
            if (response.Data == null) return NotFound(response.Message);

            return Ok(response);
        }

        [Authorize]
        [HttpPatch("profile/change-password")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var response = await _userService.ChangePassword(changePasswordDto);

            if (response.Data == false)
                return BadRequest(response.Message);
            return Ok(response);
        }

        [HttpPost("profile/change-image")]
        public async Task<ActionResult> UploadProfileImageAsync([FromForm] UploadProfileImageDto uploadProfileImageDto)
        {
            var response = await _userService.UploadProfileImageAsync(uploadProfileImageDto);

            if (response.Data == false)
                return BadRequest(response.Message);
            return Ok(response);
        }

        [HttpDelete("profile/remove-image")]
        public async Task<ActionResult> RemoveProfileImageAsync()
        {
            var response = await _userService.RemoveProfileImage();

            if (response.Data == false)
                return BadRequest(response.Message);
            return Ok(response);
        }

    }
}
