namespace UCMS.DTOs.TeamPhaseDto;

public class SortPhaseSubmissionsForInstructorDto
{
    public int PhaseId { get; set; }
    public SortPhaseSubmissionByForInstructorOption SortBy { get; set; } = SortPhaseSubmissionByForInstructorOption.None;
    public SortOrderOption SortOrder { get; set; } = SortOrderOption.Ascending;
}