namespace UCMS.DTOs.ClassDto;

public class GetClassPreviewForStudentDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string InstructorFullName { get; set; }
    public bool IsActive { get; set; }
    public int StudentCount { get; set; }
    public List<ClassScheduleDto> Schedules { get; set; }
}