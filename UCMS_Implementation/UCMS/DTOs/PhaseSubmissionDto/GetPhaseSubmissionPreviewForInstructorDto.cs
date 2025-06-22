namespace UCMS.DTOs.PhaseSubmissionDto;

public class GetPhaseSubmissionPreviewForInstructorDto
{
    public int Id { get; set; }
    public string TeamName { get; set; }
    public DateTime SubmittedAt { get; set; }
    public double? Score { get; set; }
    public string FileType { get; set; }
}