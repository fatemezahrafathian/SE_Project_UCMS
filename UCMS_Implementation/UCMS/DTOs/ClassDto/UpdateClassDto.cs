using UCMS.Models;

namespace UCMS.DTOs.ClassDto;

public class UpdateClassDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ICollection<ClassSchedule> Schedules { get; set; }
}