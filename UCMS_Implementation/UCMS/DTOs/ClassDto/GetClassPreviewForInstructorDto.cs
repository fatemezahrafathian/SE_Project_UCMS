namespace UCMS.DTOs.ClassDto;

public class GetClassPreviewForInstructorDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool IsActive { get; set; }
    public int StudentCount { get; set; }
    public List<ClassScheduleDto> Schedules { get; set; }
}