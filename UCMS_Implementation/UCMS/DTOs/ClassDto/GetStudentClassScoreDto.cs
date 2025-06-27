namespace UCMS.DTOs.ClassDto;

public class GetStudentClassScoreDto
{
    public int ClassId { get; set; }
    public string ClassTitle { get; set; }
    public double Score { get; set; }
    public double TotalScore { get; set; }
}