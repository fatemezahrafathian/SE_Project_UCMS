using UCMS.Models;

namespace UCMS.DTOs.TeamDto;

public class GetStudentTeamForStudentDto
{
    public int StudentId { get; set; }
    public string FullName { get; set; }
    public TeamRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}