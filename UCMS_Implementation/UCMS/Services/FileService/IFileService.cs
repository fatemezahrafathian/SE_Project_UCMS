using UCMS.DTOs;

namespace UCMS.Services.FileService;

public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file, string folder);
    bool IsValidExtension(IFormFile file);
    bool IsValidExtension(IFormFile file, string validExtensions);
    bool IsValidFileSize(IFormFile file);
    void DeleteFile(string relativePath);
    Task<FileDownloadDto?> DownloadFile(string relativePath);
    string? GetContentTypeFromPath(string? filePath); 
    Task<List<FileDownloadDto?>> DownloadFiles(List<string> relativePaths); 
    Task<FileDownloadDto?> DownloadFile2(string relativePath);
    Task<FileDownloadDto?> ZipFiles(Dictionary<string, string> namedFilePaths, string zipFileName);
}