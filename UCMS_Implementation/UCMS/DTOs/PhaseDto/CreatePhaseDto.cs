using UCMS.Models;

namespace UCMS.DTOs.PhaseDto;

public class CreatePhaseDto
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int PhaseScore { get; set; }
    public IFormFile? PhaseFile { get; set; }
    
    public string FileFormats { get; set; }

}