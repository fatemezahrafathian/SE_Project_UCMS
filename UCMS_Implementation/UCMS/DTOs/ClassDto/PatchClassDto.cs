using UCMS.Models;

namespace UCMS.DTOs.ClassDto;

public class PatchClassDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public IFormFile? ProfileImage { get; set; }
    public List<ClassScheduleDto>? Schedules { get; set; }
}