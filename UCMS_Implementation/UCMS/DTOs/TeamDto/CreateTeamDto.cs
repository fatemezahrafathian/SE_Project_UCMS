namespace UCMS.DTOs.TeamDto;

public class CreateTeamDto
{
    public string Name { get; set; }
    public string LeaderStudentNumber { get; set; }
    public List<string> StudentNumbers { get; set; }
}