namespace UCMS.DTOs.ClassDto;

public class GetClassPreviewDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string InstructorFullName { get; set; }
    public List<ClassScheduleDto> Schedules { get; set; }
}