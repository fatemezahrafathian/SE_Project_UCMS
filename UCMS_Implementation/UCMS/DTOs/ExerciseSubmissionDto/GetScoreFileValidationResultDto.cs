namespace UCMS.DTOs.ExerciseSubmissionDto;

public class GetScoreFileValidationResultDto
{
    public int RowNumber { get; set; }
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}