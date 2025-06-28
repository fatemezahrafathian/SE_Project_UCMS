using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using UCMS.Services.ImageService;

namespace UCMS_Test.Service;

public class ImageServiceTests
{
    private readonly Mock<IWebHostEnvironment> _mockEnv;
    private readonly IOptions<ImageUploadSettings> _mockSettings;
    private readonly ImageService _imageService;

    public ImageServiceTests()
    {
        _mockEnv = new Mock<IWebHostEnvironment>();
        string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempPath);
        _mockEnv.Setup(e => e.WebRootPath).Returns(tempPath);

        _mockSettings = Options.Create(new ImageUploadSettings
        {
            MaxFileSizeInMB = 2,
            AllowedExtensions = new string[] { ".jpg", ".jpeg", ".png", ".gif" }
        });

        _imageService = new ImageService(_mockEnv.Object, _mockSettings);
    }

    [Fact]
    public async Task SaveImageAsync_SavesFileCorrectlyAndReturnsPath()
    {
        // Arrange
        var content = "test image content";
        var fileName = "test.jpg";
        var fileMock = new FormFile(
            new MemoryStream(Encoding.UTF8.GetBytes(content)),
            0,
            content.Length,
            "image",
            fileName
        );

        var folder = "images";

        // Act
        var resultPath = await _imageService.SaveImageAsync(fileMock, folder);

        // Assert
        var expectedDirectory = Path.Combine(_mockEnv.Object.WebRootPath, folder);
        var savedFilePath = Path.Combine(_mockEnv.Object.WebRootPath, resultPath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

        Assert.True(Directory.Exists(expectedDirectory));
        Assert.True(File.Exists(savedFilePath));
        Assert.StartsWith($"/{folder}/", resultPath);

        // Clean up
        File.Delete(savedFilePath);
        Directory.Delete(expectedDirectory);
    }
    
    [Fact]
    public async Task SaveImageAsync_CreatesDirectory_IfNotExists()
    {
        // Arrange
        var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var folderName = "uploads";
        var fullFolderPath = Path.Combine(tempRoot, folderName);

        _mockEnv.Setup(e => e.WebRootPath).Returns(tempRoot);

        var imageService = new ImageService(_mockEnv.Object, _mockSettings);

        var fileContent = "image-data";
        var fileMock = new FormFile(
            new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent)),
            0,
            fileContent.Length,
            "file",
            "test.jpg"
        );

        // Act
        var relativePath = await imageService.SaveImageAsync(fileMock, folderName);

        // Assert
        Assert.True(Directory.Exists(fullFolderPath), "Directory should be created if it doesn't exist.");

        var savedFilePath = Path.Combine(tempRoot, relativePath.TrimStart('/'));
        Assert.True(File.Exists(savedFilePath), "File should exist at the saved path.");

        // Cleanup
        File.Delete(savedFilePath);
        Directory.Delete(fullFolderPath, recursive: true);
    }

    [Fact]
    public async Task SaveImageAsync_UsesCorrectGeneratedFileName()
    {
        // Arrange
        var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var folder = "images";
        var fullFolderPath = Path.Combine(tempRoot, folder);

        _mockEnv.Setup(e => e.WebRootPath).Returns(tempRoot);

        var imageService = new ImageService(_mockEnv.Object, _mockSettings);

        var originalExtension = ".jpg";
        var originalFileName = "photo" + originalExtension;
        var fileMock = new FormFile(
            new MemoryStream(new byte[10]),
            0,
            10,
            "file",
            originalFileName
        );

        // Act
        var relativePath = await imageService.SaveImageAsync(fileMock, folder);

        // Assert
        var fileName = Path.GetFileName(relativePath);

        Assert.True(Guid.TryParse(Path.GetFileNameWithoutExtension(fileName), out _), "Filename should start with a valid GUID.");
        Assert.Equal(originalExtension, Path.GetExtension(fileName), ignoreCase: true);

        var savedPath = Path.Combine(tempRoot, relativePath.TrimStart('/'));
        Assert.True(File.Exists(savedPath), "File should be created on disk.");

        // Cleanup
        File.Delete(savedPath);
        Directory.Delete(fullFolderPath);
    }
    
    [Fact]
    public async Task SaveImageAsync_SavesToCorrectFolder()
    {
        // Arrange
        var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var folder = "my-images";
        var expectedDirectory = Path.Combine(tempRoot, folder);

        _mockEnv.Setup(e => e.WebRootPath).Returns(tempRoot);

        var imageService = new ImageService(_mockEnv.Object, _mockSettings);

        var fileMock = new FormFile(
            new MemoryStream(new byte[10]), 0, 10, "file", "sample.jpg"
        );

        // Act
        var relativePath = await imageService.SaveImageAsync(fileMock, folder);

        // Assert
        var savedFileName = Path.GetFileName(relativePath);
        var fullSavedPath = Path.Combine(expectedDirectory, savedFileName);

        Assert.True(Directory.Exists(expectedDirectory), "Target directory should exist");

        Assert.True(File.Exists(fullSavedPath), "File should exist in the correct folder");

        // Cleanup
        File.Delete(fullSavedPath);
        Directory.Delete(expectedDirectory);
    }

    [Theory]
    [InlineData("photo.jpg", true)]
    [InlineData("photo.JPG", true)]
    [InlineData("image.jpeg", true)]
    [InlineData("img.JPEG", true)]
    [InlineData("pic.png", true)]
    [InlineData("animation.gif", true)]
    [InlineData("script.exe", false)]
    [InlineData("document.pdf", false)]
    [InlineData("archive.zip", false)]
    [InlineData("noextension", false)]
    public void IsValidImageExtension_ReturnsExpectedResult(string fileName, bool expected)
    {
        // Arrange
        var fileMock = new FormFile(
            new MemoryStream(new byte[0]), 0, 0, "file", fileName
        );

        // Act
        var result = _imageService.IsValidImageExtension(fileMock);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1024 * 1024 * 1, true)]   // 1MB
    [InlineData(1024 * 1024 * 2, true)]   // 2MB
    [InlineData(1024 * 1024 * 3, false)]  // 3MB
    [InlineData(0, true)]                 
    public void IsValidImageSize_ShouldReturnExpectedResult(long fileSize, bool expected)
    {
        // Arrange
        var fileMock = new FormFile(
            new MemoryStream(new byte[fileSize]), 0, fileSize, "file", "image.jpg"
        );

        // Act
        var result = _imageService.IsValidImageSize(fileMock);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void DeleteImage_DeletesFile_WhenFileExists()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var fileName = "test.jpg";
        var filePath = Path.Combine(tempDir, fileName);
        File.WriteAllText(filePath, "dummy content");

        _mockEnv.Setup(e => e.WebRootPath).Returns(tempDir);

        var relativePath = "/" + fileName;

        // Act
        _imageService.DeleteImage(relativePath);

        // Assert
        Assert.False(File.Exists(filePath));

        // Cleanup
        Directory.Delete(tempDir);
    }

    [Fact]
    public void DeleteImage_DoesNothing_WhenFileDoesNotExist()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        _mockEnv.Setup(e => e.WebRootPath).Returns(tempDir);

        var relativePath = "/nonexistent.jpg";

        // Act & Assert (should not throw)
        var exception = Record.Exception(() => _imageService.DeleteImage(relativePath));
        Assert.Null(exception);

        // Cleanup
        Directory.Delete(tempDir);
    }

    [Fact]
    public void DeleteImage_HandlesRelativePathWithAndWithoutLeadingSlash()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var fileName = "image.png";
        var filePath = Path.Combine(tempDir, fileName);
        File.WriteAllText(filePath, "dummy");

        _mockEnv.Setup(e => e.WebRootPath).Returns(tempDir);

        // Act with leading slash
        _imageService.DeleteImage("/" + fileName);
        Assert.False(File.Exists(filePath));

        // Re-create file for next test
        File.WriteAllText(filePath, "dummy");

        // Act without leading slash
        _imageService.DeleteImage(fileName);
        Assert.False(File.Exists(filePath));

        // Cleanup
        Directory.Delete(tempDir);
    }

}