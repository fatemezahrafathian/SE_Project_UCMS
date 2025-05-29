namespace UCMS.DTOs.TeamDto;

public class GetTeamTemplateFileDto
{
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public byte[] FileContent { get; set; }
}