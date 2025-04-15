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
    
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public ClassIdentifierType IdentifierType { get; set; }
    
    public ICollection<ClassSchedule> Schedules { get; set; }
}