namespace UCMS.DTOs.TeamDto;

public class GetTeamForStudentDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<GetStudentTeamForStudentDto> StudentTeams { get; set; } = new List<GetStudentTeamForStudentDto>();
}