namespace UCMS.DTOs.ExamDto;

public class GetExamForStudentDto
{
    public int examId { get; set; } 
    public string Title { get; set; }
    public string classTitle { get; set; }
    public string? ExamLocation { get; set; }
    public DateTime Date { get; set; }
    public int ExamScore { get; set; }
}