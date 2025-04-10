using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.DTOs.AuthDto;
using UCMS.Models;
using UCMS.Resources;
using UCMS.Services.AuthService.Abstraction;

namespace UCMS.Controllers;
[Route("api/auth")]
[ApiController]
public class AuthController: ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IPasswordService _passwordService;
    public AuthController(IAuthService authService, IPasswordService passwordService)
    {
        _authService = authService;
        _passwordService=passwordService;
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
    
    // to test authorization attribute
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = HttpContext.Items["User"] as User;
        // user = null; // to test exception middleware
        var username = user.Username;

        return Ok(new { RequestedId = id, CurrentUsername = username });    
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        // try
        // {
            var response = await _authService.Login(request);
            if (response.Success)
            {
                return Ok(new { message = response.Message }); 
            }
            if (response.Message=="InvalidInputMessage")
            {
                return BadRequest(new { message = response.Message }); 
            }
            if (response.Message=="UserNotFoundMessage")
            {
                return NotFound(new { message = response.Message });
            }
            return Unauthorized(new { message = response.Message });
        // }
        // catch (Exception ex)
        // {
        //     return Problem(
        //         detail: ex.Message,
        //         statusCode: 500,
        //         title: Messages.InternalServerError,
        //         instance: HttpContext.Request.Path
        //     );
        // }
        
    }
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var response =await _authService.Logout();
        return Ok(new { message = response.Message });
    }
    [HttpGet("status")]
    public async Task<IActionResult> GetAuthorized()
    {
        var response = await _authService.GetAuthorized();
        if (!response.Success)
        {
            return Unauthorized(new { message = response.Message });
        }
        return Ok(new { message = response.Message });
    }
    [HttpPost("RequestTempPassword")]
    public async Task<IActionResult> RequestTempPassword([FromBody] ForgetPasswordDto forgetPasswordDto)
    {
        var response = await _passwordService.RequestPasswordResetAsync(forgetPasswordDto);
        if (!response.Success)
            return BadRequest(new { message = response.Message });

        return Ok(new { message = response.Message });
    }

    [HttpPost("TempPassword")]
    public async Task<IActionResult> TempPassword([FromBody] ResetPasswordDto dto)
    {
        var response = await _passwordService.TempPasswordAsync(dto);
        if (!response.Success)
            return BadRequest(new { message = response.Message });

        return Ok(new { message = response.Message });
    }
}