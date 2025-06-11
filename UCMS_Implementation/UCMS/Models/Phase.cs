using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UCMS.Models;

public class Phase
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
    public int PhaseScore { get; set; }
    public string? PhaseFilePath { get; set; } 
    [Required]
    public string? FileFormats { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [Required]
    public int ProjectId { get; set; }
    [ForeignKey("ProjectId")]
    public Project Project { get; set; }
    public ICollection<StudentTeamPhase> StudentTeamPhases { get; set; } = new List<StudentTeamPhase>();
}