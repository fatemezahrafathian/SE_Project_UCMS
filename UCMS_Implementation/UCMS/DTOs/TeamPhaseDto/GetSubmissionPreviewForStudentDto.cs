namespace UCMS.DTOs.TeamPhaseDto;

public class GetSubmissionPreviewForStudentDto
{
    public int Id { get; set; }
    public DateTime SubmittedAt { get; set; }
    public bool IsFinal { get; set; }
    public string FileType { get; set; }
    public string? Description { get; set; }
}