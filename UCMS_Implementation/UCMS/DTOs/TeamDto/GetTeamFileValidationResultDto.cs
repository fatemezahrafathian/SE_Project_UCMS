namespace UCMS.DTOs.TeamDto;

public class GetTeamFileValidationResultDto
{
    public int RowNumber { get; set; }
    public CreateTeamDto Team { get; set; }
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}