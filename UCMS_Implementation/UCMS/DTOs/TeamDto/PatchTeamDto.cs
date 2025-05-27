namespace UCMS.DTOs.TeamDto;

public class PatchTeamDto
{
    public string? Name { get; set; }
    public string? LeaderStudentNumber { get; set; }
    public List<string>? AddedStudentNumbers { get; set; }
    public List<string>? DeletedStudentNumbers { get; set; }
}