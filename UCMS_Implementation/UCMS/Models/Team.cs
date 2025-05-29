using System.ComponentModel.DataAnnotations;

namespace UCMS.Models;

public class Team
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public int ProjectId { get; set; }
    public Project Project { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<StudentTeam> StudentTeams { get; set; } = new List<StudentTeam>();
}
