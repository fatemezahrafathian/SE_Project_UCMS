namespace UCMS.DTOs.ProjectDto;

public class FilterProjectsForStudentDto
{
    public string? Title { get; set; }         
    public string? ClassTitle { get; set; }     
    public int? ProjectStatus { get; set; }
    public string OrderBy { get; set; } = "EndDate";
    public bool Descending { get; set; } = true; 
}