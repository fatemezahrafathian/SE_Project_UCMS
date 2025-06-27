namespace UCMS.DTOs.ClassDto;

public class CreateClassDto
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public IFormFile? ProfileImage { get; set; }
    public List<ClassScheduleDto> Schedules { get; set; } = new();
}