using System.ComponentModel.DataAnnotations;

namespace UCMS.DTOs.AuthDto;

public class LoginDto
{
    [Required, MaxLength(100)]
    public string Email { get; init; }
    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; init; }
}