namespace UCMS.DTOs.ClassDto;

public class UpdateClassEntriesDto
{
    public List<UpdateClassEntryDto> EntryDtos { get; set; }
    public double TotalScore { get; set; }
}