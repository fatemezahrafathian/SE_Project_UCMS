namespace UCMS.Models;

public class StudentTeam
{
    public int Id { get; set; }

    public int TeamId { get; set; }
    public Team Team { get; set; }

    public int StudentId { get; set; }
    public Student Student { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public TeamRole Role { get; set; } = TeamRole.Member;
}