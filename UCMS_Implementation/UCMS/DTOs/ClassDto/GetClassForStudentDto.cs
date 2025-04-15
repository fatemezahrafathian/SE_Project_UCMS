namespace UCMS.DTOs.ClassDto;

public class GetClassForStudentDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } // nullable or not
    public string InstructorFullName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public int StudentCount { get; set; }
    public List<ClassScheduleDto> Schedules { get; set; }

}