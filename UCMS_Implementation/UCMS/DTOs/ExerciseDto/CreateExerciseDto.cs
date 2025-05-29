namespace UCMS.DTOs.ExerciseDto;

public class CreateExerciseDto
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int ExerciseScore { get; set; }
    public IFormFile? ExerciseFile { get; set; }
    
    public string FileFormats { get; set; } 
}