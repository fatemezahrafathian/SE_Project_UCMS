using System.ComponentModel.DataAnnotations;
using UCMS.Models;

public class PhaseSubmission
{
    public int Id { get; set; }

    [Required]
    public int StudentTeamPhaseId { get; set; }
    public StudentTeamPhase StudentTeamPhase { get; set; }

    [Required, MaxLength(300)]
    public string FilePath { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsFinal { get; set; } = false;
    
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}