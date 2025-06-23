namespace UCMS.DTOs.ClassDto;

public class GetClassStudentScoresDto
{
    public int StudentId { get; set; }
    public string FullName { get; set; }
    public string StudentNumber { get; set; }
    public List<double> Scores { get; set; } = new List<double>();
}