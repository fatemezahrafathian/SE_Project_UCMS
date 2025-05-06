namespace UCMS.Services.FileService;

public class FileUploadSettings
{
    public List<string> AllowedExtensions { get; set; } = new();
    public int MaxFileSizeInMB { get; set; } = 100*1024*1024;
}
