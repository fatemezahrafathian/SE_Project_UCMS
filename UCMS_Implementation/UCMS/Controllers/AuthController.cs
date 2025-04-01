using Microsoft.AspNetCore.Mvc;
using UCMS.DTOs.AuthDto;
using UCMS.Resources;
using UCMS.Services.AuthService.Abstraction;

namespace UCMS.Controllers;
[Route("api/auth")]
[ApiController]
// role system
// email service
// email service test
public class AuthController: ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            var response = await _authService.Register(registerDto);

            if (response.Success)
            {
                return CreatedAtAction(nameof(GetUserById), new { id = response.Data }, response);
            }

            return BadRequest(new { message = response.Message });
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: 500,
                title: Messages.InternalServerError,
                instance: HttpContext.Request.Path
            );
        }
    }

    // cleanup and mach the way they return, add try catch
    [HttpGet("confirm")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
    {
        var response = await _authService.ConfirmEmail(token);

        if (!response.Success)
            return BadRequest(response.Message);

        return Ok(response.Message);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        return Ok(1);
    }
}