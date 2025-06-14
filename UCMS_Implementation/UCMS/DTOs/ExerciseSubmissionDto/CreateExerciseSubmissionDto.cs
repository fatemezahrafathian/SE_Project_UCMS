namespace UCMS.DTOs.ExerciseSubmissionDto;

public class CreateExerciseSubmissionDto
{
    public IFormFile? SubmissionFile { get; set; }
    public string? Description { get; set; }
}