namespace UCMS.DTOs.PhaseSubmissionDto;

public class GetSubmissionFileDto
{
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public byte[] FileContent { get; set; }
}