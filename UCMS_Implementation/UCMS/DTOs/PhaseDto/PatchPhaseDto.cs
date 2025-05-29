namespace UCMS.DTOs.PhaseDto;

public class PatchPhaseDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? PhaseScore { get; set; }
    public IFormFile? PhaseFile { get; set; }
    public string? FileFormats { get; set; }
}