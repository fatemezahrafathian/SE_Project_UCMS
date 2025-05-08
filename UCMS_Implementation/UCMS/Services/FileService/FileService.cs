using UCMS.DTOs;
using UCMS.Factories;
using UCMS.Resources;

namespace UCMS.Services.FileService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

public class FileService: IFileService
{
    private readonly IWebHostEnvironment _env;
    private readonly FileUploadSettings _settings;

    public FileService(IWebHostEnvironment env, IOptions<FileUploadSettings> settings)
    {
        _env = env;
        _settings = settings.Value;
    }
    public async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        var fileName = GenerateFileName(file.FileName);
        var savePath = Path.Combine(_env.WebRootPath, folder);

        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        var fullPath = Path.Combine(savePath, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/{folder}/{fileName}";
    }
    
    public void DeleteFile(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;

        var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
    
    public bool IsValidExtension(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return _settings.AllowedExtensions.Contains(extension);
    }
    
    public bool IsValidFileSize(IFormFile file)
    {
        var maxSize = _settings.MaxFileSizeInMB * 1024 * 1024;
        return file.Length <= maxSize;
    }
    private string GenerateFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        return $"{Guid.NewGuid()}{extension}";
    }
    public async Task<FileDownloadDto?> DownloadFile(string relativePath)
    {
        
        var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));
        if (!File.Exists(fullPath))
            return null;
        var fileBytes = await File.ReadAllBytesAsync(fullPath);
        var fileName = Path.GetFileName(relativePath);

        var dto = new FileDownloadDto
        {
            FileBytes = fileBytes,
            FileName = fileName
        };
        return dto;
    }
}

