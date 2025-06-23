namespace UCMS.DTOs;

public class FileDownloadDto
{
    public byte[] FileBytes { get; set; } = null!;
    public string? ContentType { get; set; } = "application/octet-stream";
    public string FileName { get; set; } = "download";
}
