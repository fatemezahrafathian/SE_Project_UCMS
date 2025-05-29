namespace UCMS.DTOs.ExerciseDto;

public class GetExerciseForStudentDto
{
    public int exerciseId { get; set; } 
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int ExerciseScore { get; set; }
    public string? ExerciseFilePath { get; set; } 
    public string? FileFormats { get; set; }
    // public string? PhaseFileContentType { get; set; }
    // public PhaseStatus PhaseStatus { get; set; }
}
