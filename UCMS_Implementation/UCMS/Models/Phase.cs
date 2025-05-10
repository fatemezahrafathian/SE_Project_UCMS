using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UCMS.Models;

public class Phase
{
    [Key]
    public int Id { get; set; } 
    public string? Description { get; set; }
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
    public int PhaseScore { get; set; }
    public string? ProjectFilePath { get; set; } 

    public string? FileFormats { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [Required]
    public int ProjectId { get; set; }
    [ForeignKey("ProjectId")]
    public Project Project { get; set; }
}