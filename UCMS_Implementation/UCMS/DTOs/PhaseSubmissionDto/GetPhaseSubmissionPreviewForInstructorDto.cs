namespace UCMS.DTOs.TeamPhaseDto;

public class GetPhaseSubmissionPreviewForInstructorDto
{
    public int Id { get; set; }
    public string TeamName { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string FileType { get; set; }
    public string? Description { get; set; }
}