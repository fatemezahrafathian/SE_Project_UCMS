namespace UCMS.DTOs.PhaseSubmissionDto;

public class GetPhaseSubmissionPreviewForInstructorDto
{
    public int Id { get; set; }
    public string TeamName { get; set; }
    public int TeamId { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string FileType { get; set; }
    public string? Description { get; set; }
}