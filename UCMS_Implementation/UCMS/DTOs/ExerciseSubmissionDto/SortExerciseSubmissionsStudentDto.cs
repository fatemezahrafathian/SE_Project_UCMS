using UCMS.DTOs.PhaseSubmissionDto;

namespace UCMS.DTOs.ExerciseSubmissionDto;

public class SortExerciseSubmissionsStudentDto
{
    public int ExerciseId { get; set; }
    public SortExerciseSubmissionByForStudentOption SortBy { get; set; } = SortExerciseSubmissionByForStudentOption.None;
    public SortOrderOption SortOrder { get; set; } = SortOrderOption.None;
}