namespace UCMS.DTOs.ClassDto;

public class GetClassStudentsScoresDto
{
    public List<GetClassEntryPreviewDto> headers { get; set; }
    public List<GetClassStudentScoresDto> ClassStudentScoresDtos { get; set; } = new List<GetClassStudentScoresDto>();
}