using UCMS.Models;

namespace UCMS.DTOs.ClassDto;

public class PatchClassDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public IFormFile? ProfileImage { get; set; }
    public List<ClassScheduleDto>? Schedules { get; set; }
}