namespace UCMS.DTOs.ClassDto;

public class GetClassForInstructorDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string ClassCode { get; set; }
    public bool IsActive { get; set; }
    public string? ProfileImageUrl { get; set; }
    public int StudentCount { get; set; } = 0;
    public List<ClassScheduleDto> Schedules { get; set; }
}