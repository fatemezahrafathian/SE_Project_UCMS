using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UCMS.Models;

public class Exam
{
    [Key]
    public int Id { get; set; } 
    [Required, MaxLength(100)]
    public string Title { get; set; }
    [MaxLength(500)]
    public string? ExamLocation { get; set; }
    [Required]
    public DateTime Date { get; set; }
    [Required]
    public int ExamScore { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [Required]
    public int ClassId { get; set; }
    [ForeignKey("ClassId")]
    public Class Class { get; set; } = null!;
}