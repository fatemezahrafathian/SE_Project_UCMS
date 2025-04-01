using System.ComponentModel.DataAnnotations;

namespace UCMS.Models;

public class User
{
    [Key]
    public int Id { get; set; }
    
    [Required, MaxLength(100), EmailAddress] // automated email validation, to be tested
    public string Email { get; set; }

    [Required, MaxLength(20)]
    public string Username { get; set; }

    [Required, MaxLength(256)]  // only 16 bytes needed, to work better
    public byte[] PasswordSalt { get; set; }

    [Required, MaxLength(256)] // only 32 bytes needed, to work better
    public byte[] PasswordHash { get; set; }
    
    // public bool EmailConfirmed { get; set; } = false;
    
    [MaxLength(50)]
    public string? FirstName { get; set; }

    [MaxLength(50)]
    public string? LastName { get; set; }

    public Gender? Gender { get; set; }
   
    [MaxLength(250)]
    public string? Address { get; set; }
    
    [MaxLength(500)]
    public string? Bio { get; set; }
    
    [MaxLength(200)]
    public string? ProfileImagePath { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime DateOfBirth { get; set; }
    public DateTime LastLogin { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength((32 / 3 + (32 % 3 == 0 ? 0 : 1)) * 4)]  // conf file
    public string? VerificationToken { get; set; }
    public bool IsConfirmed { get; set; }
    // only for required attibutes
    // public User(string email, string username, byte[] passwordSalt, byte[] passwordHash)
    // {
    //     Email = email;
    //     Username = username;
    //     PasswordSalt = passwordSalt;
    //     PasswordHash = passwordHash;
    // }
}