using UCMS.Models;

namespace UCMS.DTOs.TeamDto;

public class GetStudentTeamForInstructorDto
{
    public int StudentId { get; set; }
    public string StudentNumber { get; set; }
    public string FullName { get; set; }
    public TeamRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}