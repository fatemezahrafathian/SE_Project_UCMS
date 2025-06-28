using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using UCMS.Models;
using UCMS.Services.FileService;

namespace UCMS_Test.Service;

public class FileServiceTests
{
    private readonly Mock<IWebHostEnvironment> _mockEnv;
    private readonly IOptions<FileUploadSettings> _fileSettings;
    private readonly IOptions<MimeTypeOptions> _mimeOptions;
    private readonly FileService _fileService;

    public FileServiceTests()
    {
        _mockEnv = new Mock<IWebHostEnvironment>();
        _mockEnv.Setup(env => env.WebRootPath).Returns("wwwroot");
        
        _fileSettings = Options.Create(new FileUploadSettings
        {
            MaxFileSizeInMB = 10 * 1024 * 1024,
            AllowedExtensions = new List<string>() {".pdf", ".zip", ".docx"}
        });

        _mimeOptions = Options.Create(new MimeTypeOptions
        {
            MimeTypes = new Dictionary<string, string>
            {
                { ".pdf", "application/pdf" },
                { ".zip", "application/zip" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { ".jpg", "image/jpeg" },
                { ".png", "image/png" }            }
        });
        
        _fileService = new FileService(_mockEnv.Object, _fileSettings, _mimeOptions);
    }
    
    [Fact]
    public async Task SaveFileAsync_Should_SaveFileInCorrectPath()
    {
        // Arrange
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

        var options = Options.Create(new FileUploadSettings());
        var mimeOptions = Options.Create(new MimeTypeOptions { MimeTypes = new Dictionary<string, string>() });

        var service = new FileService(envMock.Object, options, mimeOptions);
        var folder = "test-folder";

        var fileName = "test.txt";
        var fileContent = "Hello World";
        var fileMock = new FormFile(
            new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent)),
            0,
            fileContent.Length,
            "Data",
            fileName
        );

        // Act
        var path = await service.SaveFileAsync(fileMock, folder);

        // Assert
        var savedFilePath = Path.Combine(envMock.Object.WebRootPath, folder, Path.GetFileName(path));
        Assert.True(File.Exists(savedFilePath));

        // Clean up
        File.Delete(savedFilePath);
        Directory.Delete(Path.Combine(envMock.Object.WebRootPath, folder));
    }

    [Fact]
    public async Task SaveFileAsync_Should_CreateDirectory_IfNotExist()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.WebRootPath).Returns(tempPath);

        var options = Options.Create(new FileUploadSettings());
        var mimeOptions = Options.Create(new MimeTypeOptions());

        var service = new FileService(envMock.Object, options, mimeOptions);

        var fileContent = "sample";
        var fileMock = new FormFile(
            new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent)),
            0, fileContent.Length,
            "file", "file.txt"
        );

        var folder = "new-dir";

        // Act
        var returnedPath = await service.SaveFileAsync(fileMock, folder);

        // Assert
        var fullFolderPath = Path.Combine(tempPath, folder);
        Assert.True(Directory.Exists(fullFolderPath));

        // Clean up
        File.Delete(Path.Combine(tempPath, folder, Path.GetFileName(returnedPath)));
        Directory.Delete(fullFolderPath);
    }

    [Fact]
    public async Task SaveFileAsync_Should_ReturnCorrectRelativePath()
    {
        // Arrange
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

        var options = Options.Create(new FileUploadSettings());
        var mimeOptions = Options.Create(new MimeTypeOptions());

        var service = new FileService(envMock.Object, options, mimeOptions);

        var fileMock = new FormFile(new MemoryStream(new byte[5]), 0, 5, "file", "example.txt");
        var folder = "myfiles";

        // Act
        var returnedPath = await service.SaveFileAsync(fileMock, folder);

        // Assert
        Assert.StartsWith("/" + folder + "/", returnedPath);
    }

    [Fact]
    public void DeleteFile_Should_DeleteFile_IfExists()
    {
        // Arrange
        var folder = "delete-test-folder";
        var fileName = "delete-test-file.txt";
        var tempPath = Path.Combine(Path.GetTempPath(), folder);
        Directory.CreateDirectory(tempPath);

        var fullFilePath = Path.Combine(tempPath, fileName);
        File.WriteAllText(fullFilePath, "test");

        _mockEnv.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

        var relativePath = "/" + folder + "/" + fileName;

        // Act
        _fileService.DeleteFile(relativePath);

        // Assert
        Assert.False(File.Exists(fullFilePath));

        // Clean up
        if (Directory.Exists(tempPath)) Directory.Delete(tempPath);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DeleteFile_Should_DoNothing_WhenPathIsNullOrWhiteSpace(string? relativePath)
    {
        // Act & Assert
        var exception = Record.Exception(() => _fileService.DeleteFile(relativePath));
        Assert.Null(exception);
    }

    [Fact]
    public void DeleteFile_Should_DoNothing_WhenFileDoesNotExist()
    {
        // Arrange
        var relativePath = "/non-existing-folder/non-existing-file.txt";

        _mockEnv.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

        // Act & Assert
        var exception = Record.Exception(() => _fileService.DeleteFile(relativePath));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("document.pdf", true)]
    [InlineData("script.exe", false)]
    [InlineData("noextension", false)]
    [InlineData("resume.pdf", true)]           // With custom list
    [InlineData("malware.exe", false)]         // With custom list
    [InlineData("anything.txt", false)]        // With empty list
    public void IsValidExtension_WorksAsExpected(string fileName, bool expected)
    {
        // Arrange
        var fileMock = new FormFile(
            new MemoryStream(Array.Empty<byte>()), 0, 0, "file", fileName
        );

        string validExtensions;
        if (fileName == "resume.pdf" || fileName == "malware.exe")
            validExtensions = "pdf,docx,zip";
        else if (fileName == "anything.txt")
            validExtensions = "";
        else
            validExtensions = null!; // Will use default _settings.AllowedExtensions

        // Act
        bool result = validExtensions == null
            ? _fileService.IsValidExtension(fileMock)
            : _fileService.IsValidExtension(fileMock, validExtensions);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("report.pdf", "pdf,docx,zip", true)]
    [InlineData("document.docx", "pdf,docx,zip", true)]
    [InlineData("archive.zip", "pdf,docx,zip", true)]
    [InlineData("script.exe", "pdf,docx,zip", false)]
    [InlineData("image.png", "jpg,jpeg,gif", false)]
    [InlineData("test", "pdf,docx,zip", false)]
    [InlineData("something.PDF", "pdf,docx", true)] // Case-insensitive
    [InlineData("note.txt", "", false)]             // Empty list
    [InlineData("test.pdf", "  pdf , zip ", true)]  // Extra spaces
    public void IsValidExtension_WithStringList_WorksAsExpected(string fileName, string validExtensions, bool expectedResult)
    {
        // Arrange
        var fileMock = new FormFile(
            new MemoryStream(new byte[0]), 0, 0, "file", fileName
        );

        // Act
        var result = _fileService.IsValidExtension(fileMock, validExtensions);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(5 * 1024 * 1024, 10, true)]   // 5MB file, 10MB limit => valid
    [InlineData(10 * 1024 * 1024, 10, true)]  // 10MB file, 10MB limit => edge case
    [InlineData(11 * 1024 * 1024, 10, false)] // 11MB file, 10MB limit => too large
    [InlineData(0, 10, true)]                // 0B file => always valid
    public void IsValidFileSize_WorksAsExpected(long fileSizeBytes, int maxSizeInMB, bool expected)
    {
        // Arrange
        var mockSettings = Options.Create(new FileUploadSettings
        {
            MaxFileSizeInMB = maxSizeInMB,
            AllowedExtensions = new List<string> { ".pdf" }
        });

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.WebRootPath).Returns("wwwroot");

        var mimeOptions = Options.Create(new MimeTypeOptions
        {
            MimeTypes = new Dictionary<string, string> { { ".pdf", "application/pdf" } }
        });

        var fileService = new FileService(envMock.Object, mockSettings, mimeOptions);

        var fileMock = new FormFile(new MemoryStream(new byte[fileSizeBytes]), 0, fileSizeBytes, "file", "example.pdf");

        // Act
        var result = fileService.IsValidFileSize(fileMock);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task DownloadFile_ReturnsNull_WhenFileDoesNotExist()
    {
        // Arrange
        var relativePath = "nonexistent/file.txt";
        _mockEnv.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

        var service = new FileService(_mockEnv.Object, _fileSettings, _mimeOptions);

        // Act
        var result = await service.DownloadFile(relativePath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DownloadFile_ReturnsFileDownloadDto_WhenFileExists()
    {
        // Arrange
        var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(folder);

        var fileName = "testfile.txt";
        var fullPath = Path.Combine(folder, fileName);
        var content = System.Text.Encoding.UTF8.GetBytes("Hello World");
        await File.WriteAllBytesAsync(fullPath, content);

        var relativePath = Path.Combine(Path.GetFileName(folder), fileName);
        _mockEnv.Setup(e => e.WebRootPath).Returns(folder);

        var service = new FileService(_mockEnv.Object, _fileSettings, _mimeOptions);

        // Act
        var result = await service.DownloadFile(fileName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fileName, result!.FileName);
        Assert.Equal(content, result.FileBytes);

        // Cleanup
        File.Delete(fullPath);
        Directory.Delete(folder);
    }

    [Fact]
    public async Task DownloadFile2_ReturnsNull_WhenFileDoesNotExist()
    {
        // Arrange
        var relativePath = "nonexistent/file.txt";
        _mockEnv.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

        // Act
        var result = await _fileService.DownloadFile2(relativePath);

        // Assert
        Assert.Null(result);
    }

[Fact]
    public async Task DownloadFile2_ReturnsDtoWithCorrectValues_WhenFileExists()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var fileName = "testfile.pdf";
        var filePath = Path.Combine(tempDir, fileName);
        var fileContent = System.Text.Encoding.UTF8.GetBytes("Test content");
        await File.WriteAllBytesAsync(filePath, fileContent);

        _mockEnv.Setup(e => e.WebRootPath).Returns(tempDir);
        
        // Act
        var result = await _fileService.DownloadFile2(fileName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fileName, result!.FileName);
        Assert.Equal(fileContent, result.FileBytes);
        Assert.Equal("application/pdf", result.ContentType);

        // Cleanup
        File.Delete(filePath);
        Directory.Delete(tempDir);
    }

    [Fact]
    public async Task DownloadFile2_ReturnsDefaultContentType_WhenContentTypeIsNull()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var fileName = "unknownfile.unknownext";
        var filePath = Path.Combine(tempDir, fileName);
        var fileContent = System.Text.Encoding.UTF8.GetBytes("Test content");
        await File.WriteAllBytesAsync(filePath, fileContent);

        _mockEnv.Setup(e => e.WebRootPath).Returns(tempDir);

        // Act
        var result = await _fileService.DownloadFile2(fileName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("application/octet-stream", result!.ContentType);

        // Cleanup
        File.Delete(filePath);
        Directory.Delete(tempDir);
    }

    [Fact]
    public async Task ZipFiles_ReturnsZipFile_WhenFilesExist()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var file1Path = Path.Combine(tempDir, "file1.txt");
        var file2Path = Path.Combine(tempDir, "file2.txt");
        await File.WriteAllTextAsync(file1Path, "Hello");
        await File.WriteAllTextAsync(file2Path, "World");

        _mockEnv.Setup(e => e.WebRootPath).Returns(tempDir);

        var filesToZip = new Dictionary<string, string>
        {
            { "first.txt", "/file1.txt" },
            { "second.txt", "/file2.txt" }
        };
        var zipFileName = "myarchive";

        // Act
        var result = await _fileService.ZipFiles(filesToZip, zipFileName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("application/zip", result!.ContentType);
        Assert.Equal("myarchive.zip", result.FileName);
        Assert.NotEmpty(result.FileBytes);

        // Optionally: extract zipBytes and check entries
        using var ms = new MemoryStream(result.FileBytes);
        using var archive = new System.IO.Compression.ZipArchive(ms);
        var entryNames = archive.Entries.Select(e => e.Name).ToList();
        Assert.Contains("first.txt", entryNames);
        Assert.Contains("second.txt", entryNames);

        // Cleanup
        File.Delete(file1Path);
        File.Delete(file2Path);
        Directory.Delete(tempDir);
    }

    [Fact]
    public async Task ZipFiles_ShouldThrowNullReferenceException_WhenNoFilesExist()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        _mockEnv.Setup(e => e.WebRootPath).Returns(tempDir);

        var filesToZip = new Dictionary<string, string>
        {
            { "nofile.txt", "/nonexistent1.txt" },
            { "nofile2.txt", "/nonexistent2.txt" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => _fileService.ZipFiles(filesToZip, "archive"));

        // Clean up
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task ZipFiles_IgnoresNonExistentFiles()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var file1Path = Path.Combine(tempDir, "file1.txt");
        await File.WriteAllTextAsync(file1Path, "Hello");

        _mockEnv.Setup(e => e.WebRootPath).Returns(tempDir);

        var filesToZip = new Dictionary<string, string>
        {
            { "file1.txt", "/file1.txt" },
            { "missing.txt", "/nonexistent.txt" }
        };

        // Act
        var result = await _fileService.ZipFiles(filesToZip, "archive");

        // Assert
        Assert.NotNull(result);
        using var ms = new MemoryStream(result.FileBytes);
        using var archive = new System.IO.Compression.ZipArchive(ms);
        var entryNames = archive.Entries.Select(e => e.Name).ToList();

        Assert.Contains("file1.txt", entryNames);
        Assert.DoesNotContain("missing.txt", entryNames);

        // Cleanup
        File.Delete(file1Path);
        Directory.Delete(tempDir);
    }
    
    [Theory]
    [InlineData("document.pdf", "application/pdf")]
    [InlineData("photo.JPG", "image/jpeg")]  
    [InlineData("archive.zip", "application/zip")]
    [InlineData("unknownfile.abc", "application/octet-stream")] 
    [InlineData("noextension", "application/octet-stream")] 
    [InlineData("", null)] 
    [InlineData(null, null)]
    public void GetContentTypeFromPath_ReturnsExpectedContentType(string? filePath, string? expectedContentType)
    {
        // Act
        var contentType = _fileService.GetContentTypeFromPath(filePath);

        // Assert
        Assert.Equal(expectedContentType, contentType);
    }
    
}