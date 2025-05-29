using UCMS.Models;

namespace UCMS.DTOs.PhaseDto;

public class GetPhaseForInstructorDto
{
    public int phaseId { get; set; } 
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int PhaseScore { get; set; }
    public string? PhaseFilePath { get; set; } 
    public string? FileFormats { get; set; }
    // public string? PhaseFileContentType { get; set; }
    // public PhaseStatus PhaseStatus { get; set; }
}