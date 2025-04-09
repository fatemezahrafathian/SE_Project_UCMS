using System.ComponentModel.DataAnnotations;
using UCMS.Models;

namespace UCMS.DTOs.AuthDto;

public class ResetPasswordDto
{
    [Required, MaxLength(100)]
    public string Email { get; init; }
    public OneTimeCode OneTimeCode { get; set; }
}