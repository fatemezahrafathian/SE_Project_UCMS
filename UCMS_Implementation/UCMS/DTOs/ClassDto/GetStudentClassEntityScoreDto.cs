using UCMS.Models;

namespace UCMS.DTOs.ClassDto;

public class GetStudentClassEntityScoreDto
{
    public int EntryId { get; set; }
    public EntryType EntryType { get; set; }
    public string EntryName { get; set; }
    public double PartialScore { get; set; }
}