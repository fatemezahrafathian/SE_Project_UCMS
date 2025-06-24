using UCMS.Models;

namespace UCMS.DTOs.ClassDto;

public class UpdateClassEntryDto
{
    public int EntryId { get; set; }
    public EntryType EntryType { get; set; }
    public double PortionInTotalScore { get; set; }
}