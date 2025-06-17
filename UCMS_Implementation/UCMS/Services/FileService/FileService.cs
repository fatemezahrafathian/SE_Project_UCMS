using System.IO.Compression;
using UCMS.DTOs;
using UCMS.Factories;
using UCMS.Models;
using UCMS.Resources;

namespace UCMS.Services.FileService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

public class FileService: IFileService
{
    private readonly IWebHostEnvironment _env;
    private readonly FileUploadSettings _settings;
    private readonly Dictionary<string, string> _mimeTypes;

    public FileService(IWebHostEnvironment env, IOptions<FileUploadSettings> settings, IOptions<MimeTypeOptions> options)
    {
        _env = env;
        _mimeTypes = options.Value.MimeTypes;
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

    public bool IsValidExtension(IFormFile file, string validExtensions)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = validExtensions
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim().ToLowerInvariant());

        return allowedExtensions.Contains(extension);
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
    
    public async Task<FileDownloadDto?> DownloadFile2(string relativePath)
    {
        var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));

        if (!File.Exists(fullPath))
            return null;

        var fileBytes = await File.ReadAllBytesAsync(fullPath);
        var fileName = Path.GetFileName(relativePath);
        var contentType = GetContentTypeFromPath(relativePath);

        var dto = new FileDownloadDto
        {
            FileBytes = fileBytes,
            FileName = fileName,
            ContentType = contentType ?? "application/octet-stream"
        };

        return dto;
    }


    public async Task<FileDownloadDto?> ZipFiles(List<string> relativePaths)
    {
        var fileList = new List<(byte[] FileContent, string FileName)>();

        foreach (var relativePath in relativePaths)
        {
            var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));
            
            if (!File.Exists(fullPath))
                continue;

            var fileBytes = await File.ReadAllBytesAsync(fullPath);
            var fileName = Path.GetFileName(fullPath);
            fileList.Add((fileBytes, fileName));
        }

        if (!fileList.Any())
        {
            throw null;
        }

        var zipBytes = CreateZipFromFiles(fileList);

        return new FileDownloadDto
        {
            FileBytes = zipBytes,
            ContentType = "application/zip",
            FileName = $"files_{DateTime.Now:yyyyMMdd_HHmmss}.zip"
        };
    }

    private byte[] CreateZipFromFiles(List<(byte[] FileContent, string FileName)> files)
    {
        using var memoryStream = new MemoryStream();

        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var file in files)
            {
                var entry = archive.CreateEntry(file.FileName, CompressionLevel.Fastest);
                using var entryStream = entry.Open();
                using var fileStream = new MemoryStream(file.FileContent);
                fileStream.CopyTo(entryStream);
            }
        }

        return memoryStream.ToArray();
    }

    // public string? GetContentTypeFromPath(string? filePath)
    // {
    //     if (string.IsNullOrEmpty(filePath)) return null;
    //     var extension = Path.GetExtension(filePath).ToLowerInvariant();
    //     return extension switch
    //     {
    //         ".pdf" => "application/pdf",
    //         ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
    //         ".doc" => "application/msword",
    //         _ => "application/octet-stream"
    //     };
    // }

    public string? GetContentTypeFromPath(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return null;

        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        if (_mimeTypes.TryGetValue(extension, out var contentType))
            return contentType;

        return "application/octet-stream";
    }
    
    public async Task<List<FileDownloadDto?>> DownloadFiles(List<string> relativePaths)
    {
        var results = new List<FileDownloadDto?>();

        foreach (var relativePath in relativePaths)
        {
            var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));

            if (!File.Exists(fullPath))
            {
                results.Add(null);
                continue;
            }

            var fileBytes = await File.ReadAllBytesAsync(fullPath);
            var fileName = Path.GetFileName(relativePath);
            var contentType = GetContentTypeFromPath(fileName);

            var dto = new FileDownloadDto
            {
                FileBytes = fileBytes,
                FileName = fileName,
                ContentType = contentType ?? "application/octet-stream"
            };

            results.Add(dto);
        }

        return results;
    }

    
}

