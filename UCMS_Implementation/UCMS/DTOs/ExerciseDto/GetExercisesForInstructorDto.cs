using UCMS.Models;

namespace UCMS.DTOs.ExerciseDto;

public class GetExercisesForInstructorDto
{
    public int exerciseId { get; set; } 
    public string Title { get; set; }
    public string classTitle { get; set; }
    public DateTime EndDate { get; set; }
    public ExerciseStatus Status { get; set; }
    
}