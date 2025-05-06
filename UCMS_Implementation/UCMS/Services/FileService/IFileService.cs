namespace UCMS.Services.FileService;

public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file, string folder);
    bool IsValidExtension(IFormFile file);
    bool IsValidFileSize(IFormFile file);
    void DeleteFile(string relativePath);
}