namespace UCMS.DTOs.ClassDto;

public class PaginatedFilterClassForStudentDto
{
    public string Title { get; set; }
    public string? InstructorName { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 6;
}