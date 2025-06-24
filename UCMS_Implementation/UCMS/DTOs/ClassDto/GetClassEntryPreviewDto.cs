using UCMS.Models;

namespace UCMS.DTOs.ClassDto;

public class GetClassEntryPreviewDto
{
    public int EntryId { get; set; }
    public EntryType EntryType { get; set; }
    public string EntryName { get; set; }
}