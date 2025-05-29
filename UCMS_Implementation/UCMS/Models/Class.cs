using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UCMS.Models;

public class Class
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Title { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public int InstructorId { get; set; }

    [ForeignKey("InstructorId")]
    public Instructor Instructor { get; set; }
    
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    [MaxLength(20)]
    public string? ClassCode { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(300)]
    public string? ProfileImageUrl { get; set; }
    [NotMapped]
    public bool IsActive
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            return (!StartDate.HasValue && !EndDate.HasValue) ||
                   (StartDate.HasValue && !EndDate.HasValue && StartDate.Value <= today) ||
                   (!StartDate.HasValue && EndDate.HasValue && EndDate.Value >= today) ||
                   (StartDate.HasValue && EndDate.HasValue &&
                    StartDate.Value <= today && EndDate.Value >= today);
        }
    }
    
    [Required, MaxLength(256)]  // only 16 bytes needed, to work better
    public byte[] PasswordSalt { get; set; }

    [Required, MaxLength(256)] // only 32 bytes needed, to work better
    public byte[] PasswordHash { get; set; }

    public ICollection<ClassSchedule> Schedules { get; set; }
    public ICollection<ClassStudent> ClassStudents { get; set; }
    
    public ICollection<Project> Projects { get; set; }
    public ICollection<Exercise> Exercises { get; set; }
}