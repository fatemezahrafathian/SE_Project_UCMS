namespace UCMS.DTOs.PhaseSubmissionDto;

public class GetPhaseSubmissionPreviewForStudentDto
{
    public int Id { get; set; }
    public DateTime SubmittedAt { get; set; }
    public bool IsFinal { get; set; }
    public double? Score { get; set; }
    public string FileType { get; set; }
}