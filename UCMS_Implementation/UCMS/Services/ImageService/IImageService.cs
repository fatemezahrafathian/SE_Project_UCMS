namespace UCMS.Services.ImageService;

public interface IImageService
{
    Task<string> SaveImageAsync(IFormFile file, string folder);
    bool IsValidImageExtension(IFormFile file);
    bool IsValidImageSize(IFormFile file);
    void DeleteImage(string relativePath);
}