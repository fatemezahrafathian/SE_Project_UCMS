namespace UCMS.DTOs.ClassDto;

public class GetClassStudentsScoresDto
{
    public List<string> headers { get; set; }
    public List<GetClassStudentScoresDto> ClassStudentScoresDtos { get; set; } = new List<GetClassStudentScoresDto>();
}