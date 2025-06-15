namespace UCMS.DTOs.PhaseSubmissionDto;

public class SortPhaseSubmissionsStudentDto
{
    public int PhaseId { get; set; }
    public SortPhaseSubmissionByForStudentOption SortBy { get; set; } = SortPhaseSubmissionByForStudentOption.None;
    public SortOrderOption SortOrder { get; set; } = SortOrderOption.Ascending;
}