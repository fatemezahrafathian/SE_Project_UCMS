using Microsoft.Extensions.Options;

namespace UCMS.Services.ImageService;

public class ImageService: IImageService
{
    private readonly IWebHostEnvironment _env;
    private readonly ImageUploadSettings _settings;

    public ImageService(IWebHostEnvironment env, IOptions<ImageUploadSettings> settings)
    {
        _env = env;
        _settings = settings.Value;
    }
    
    public async Task<string> SaveImageAsync(IFormFile file, string folder)
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

    private string GenerateFileName(string originalFileName)
    {
        var fileExtension = Path.GetExtension(originalFileName);
        return $"{Guid.NewGuid()}{fileExtension}";
    }
    
    public bool IsValidImageExtension(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return _settings.AllowedExtensions.Contains(extension);
    }

    public bool IsValidImageSize(IFormFile file)
    {
        var maxSize = _settings.MaxFileSizeInMB * 1024 * 1024;
        return file.Length <= maxSize;
    }
    
    public void DeleteImage(string relativePath)
    {
        var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
    
}