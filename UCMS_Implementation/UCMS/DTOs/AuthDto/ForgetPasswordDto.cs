using System.ComponentModel.DataAnnotations;

namespace UCMS.DTOs.AuthDto;

public class ForgetPasswordDto
{
    [Required, MaxLength(100)]
    public string Email { get; init; }
}