using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UCMS.Models;

public class Exercise
{
    [Key]
    public int Id { get; set; } 
    [Required, MaxLength(100)]
    public string Title { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
    [Required]
    public int ExerciseScore { get; set; }
    public string? ExerciseFilePath { get; set; } 
    [Required]
    public string FileFormats { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [Required]
    public int ClassId { get; set; }
    [ForeignKey("ClassId")]
    public Class Class { get; set; } = null!;
}