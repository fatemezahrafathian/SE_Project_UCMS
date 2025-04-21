namespace UCMS.DTOs.ClassDto;

public class GetClassPageDto
{
    public List<GetClassPreviewForInstructorDto> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}