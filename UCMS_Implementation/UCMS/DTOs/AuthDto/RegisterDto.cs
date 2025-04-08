using System.ComponentModel.DataAnnotations;
using UCMS.Models;

namespace UCMS.DTOs.AuthDto;

public class RegisterDto
{
    [Required, MaxLength(100), EmailAddress]
    public string Email { get; set; } // nullable or what

    [Required, MaxLength(20)]
    public string Username { get; set; }

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; }

    [Required, Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; }
    [Required]
    public Role Role { get; set; }=Role.Student;
}