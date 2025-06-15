using UCMS.DTOs.PhaseSubmissionDto;

namespace UCMS.DTOs.ExerciseSubmissionDto;

public class SortExerciseSubmissionsForInstructorDto
{
    public int ExerciseId { get; set; }
    public SortExerciseSubmissionByForInstructorOption SortBy { get; set; } = SortExerciseSubmissionByForInstructorOption.None;
    public SortOrderOption SortOrder { get; set; } = SortOrderOption.Ascending;
}