using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UCMS.Models;

public class StudentTeamPhase
{
    public int Id { get; set; }

    [Required]
    public int StudentTeamId { get; set; }
    public StudentTeam StudentTeam { get; set; }

    [Required]
    public int PhaseId { get; set; }
    public Phase Phase { get; set; }

    public double? Score { get; set; }
    
    public ICollection<PhaseSubmission> Submissions { get; set; } = new List<PhaseSubmission>();
}