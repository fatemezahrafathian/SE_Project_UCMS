using UCMS.Models;

namespace UCMS.DTOs.ClassDto;

public class GetClassDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string ClassCode { get; set; }
    public bool IsActive { get; set; }
    public ClassIdentifierType IdentifierType { get; set; }
    
    public string InstructorFullName { get; set; }
    
    public List<ClassScheduleDto> Schedules { get; set; }
}


