namespace UCMS.DTOs.TeamDto;

public class GetTeamForInstructorDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<GetStudentTeamForInstructorDto> StudentTeams { get; set; } = new List<GetStudentTeamForInstructorDto>();
}