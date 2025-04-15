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
    
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [MaxLength(20)]
    public string? ClassCode { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(300)]
    public string? ProfileImageUrl { get; set; }
    [NotMapped]
    public bool IsActive =>
        (!StartDate.HasValue && !EndDate.HasValue) ||
        (StartDate.HasValue && !EndDate.HasValue && StartDate.Value <= DateTime.UtcNow) ||
        (!StartDate.HasValue && EndDate.HasValue && EndDate.Value >= DateTime.UtcNow) ||
        (StartDate.HasValue && EndDate.HasValue &&
         StartDate.Value <= DateTime.UtcNow && EndDate.Value >= DateTime.UtcNow);

    
    // [Required] // remove this
    // public ClassIdentifierType IdentifierType { get; set; } = 0;
    
    public ICollection<ClassSchedule> Schedules { get; set; }
}