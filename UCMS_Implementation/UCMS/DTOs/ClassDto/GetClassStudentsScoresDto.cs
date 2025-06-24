namespace UCMS.DTOs.ClassDto;

public class GetClassStudentsScoresDto
{
    public List<GetClassEntryPreviewDto> headers { get; set; } = new List<GetClassEntryPreviewDto>();
    public List<GetClassStudentScoresDto> ClassStudentScoresDtos { get; set; } = new List<GetClassStudentScoresDto>();
}