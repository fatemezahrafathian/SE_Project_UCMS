namespace UCMS.DTOs.PhaseSubmissionDto;

public class CreatePhaseSubmissionDto
{
    public IFormFile? SubmissionFile { get; set; }
    public string? Description { get; set; }
}