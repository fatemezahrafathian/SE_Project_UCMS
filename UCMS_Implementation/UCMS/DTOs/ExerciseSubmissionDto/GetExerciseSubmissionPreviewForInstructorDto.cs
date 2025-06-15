namespace UCMS.DTOs.ExerciseSubmissionDto;

public class GetExerciseSubmissionPreviewForInstructorDto
{
    public int Id { get; set; }
    public string StudentName { get; set; }
    public string StudentNumber { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string FileType { get; set; }
    public string? Description { get; set; }
}