namespace UCMS.DTOs.ClassDto;

public class GetClassEntriesDto
{
    public List<GetClassEntryDto> EntryDtos { get; set; }
    public double SumOfSPartialScores { get; set; }
}