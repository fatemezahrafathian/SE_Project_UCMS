namespace UCMS.DTOs.ExamDto;

public class CreateExamDto
{
    public string Title { get; set; }
    public string? ExamLocation { get; set; }
    public DateTime Date { get; set; }
    public int ExamScore { get; set; }
}