namespace UCMS.DTOs.ProjectDto;

public class CreateProjectDto
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public int TotalScore { get; set; }
    public int ProjectType { get; set; } 
    public int? GroupSize { get; set; } 
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public IFormFile? ProjectFile { get; set; }
}