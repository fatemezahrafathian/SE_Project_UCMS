using UCMS.Models;

namespace UCMS.DTOs.ProjectDto;

public class GetProjectForInstructorDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? ProjectFilePath { get; set; }
    public string? ProjectFileContentType { get; set; }
    public ProjectStatus ProjectStatus { get; set; }
    public ProjectType ProjectType { get; set; }
    public int TotalScore { get; set; }

}