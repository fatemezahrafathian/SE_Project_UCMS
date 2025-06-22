namespace UCMS.DTOs.PhaseSubmissionDto;

public class SortPhaseSubmissionsForInstructorDto
{
    public int PhaseId { get; set; }
    public SortPhaseSubmissionByForInstructorOption SortBy { get; set; } = SortPhaseSubmissionByForInstructorOption.None;
    public SortOrderOption SortOrder { get; set; } = SortOrderOption.None;
}