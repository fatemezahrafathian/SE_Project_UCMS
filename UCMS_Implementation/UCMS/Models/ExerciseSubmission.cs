using System.ComponentModel.DataAnnotations;

namespace UCMS.Models;

public class ExerciseSubmission
{
    public int Id { get; set; }

    [Required]
    public int StudentId { get; set; }
    public Student Student { get; set; }

    [Required]
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; }

    [Required, MaxLength(300)]
    public string FilePath { get; set; }

    public double? Score { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsFinal { get; set; } = false;
    
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}