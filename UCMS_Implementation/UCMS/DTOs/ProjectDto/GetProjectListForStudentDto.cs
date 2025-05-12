using UCMS.Models;

namespace UCMS.DTOs.ProjectDto;

public class GetProjectListForStudentDto
{
    public int Id { get; set; }          
    public string Title { get; set; }   
    public string ClassTitle { get; set; }   
    public DateTime DueDate { get; set; }        
    public TimeSpan DueTime { get; set; }        
    public ProjectStatus Status { get; set; } 
}