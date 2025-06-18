using System.Text;
using Microsoft.AspNetCore.Http;
using Moq;
using UCMS.DTOs.PhaseDto;
using UCMS.Resources;
using UCMS.Services.FileService;
using UCMS.Services.PhaseService;

namespace UCMS_Test.Service;

public class CreatePhaseValidatorTest
{
    private readonly CreatePhaseDtoValidator _validator;
    private readonly Mock<IFileService> _fileServiceMock;

    public CreatePhaseValidatorTest()
    {
        _fileServiceMock = new Mock<IFileService>();
        _fileServiceMock.Setup(x => x.IsValidExtension(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(x => x.IsValidFileSize(It.IsAny<IFormFile>())).Returns(true);

        _validator = new CreatePhaseDtoValidator(_fileServiceMock.Object);
    }
    // Rule: Title is required
    [Fact]
    public async Task Should_Have_Error_When_Title_Is_Empty()
    {
        var dto = new CreatePhaseDto
        {
            Title = "",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 1
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.TitleIsRequired);

    }
    // Rule: Title max length
    [Fact]
    public async Task Should_Have_Error_When_Title_Too_Long()
    {
        var dto = new CreatePhaseDto
        {
            Title = new string('a', 101),
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 1
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.TitleMaxLength);
    }
    // Rule: Description max length
    [Fact]
    public async Task Should_Have_Error_When_Description_Too_Long()
    {
        var dto = new CreatePhaseDto
        {
            Title = "test",
            Description = new string('a', 501),
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 1
        };
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.DescriptionMaxLength);
    }
    // Rule: PhaseScore > 0
    [Fact]
    public async Task Should_Have_Error_When_PhaseScore_Is_Zero_Or_Less()
    {
        var dto = new CreatePhaseDto
        {
            Title = "test",
            Description = "test",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 0
        };
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.PhaseScoreMustBePositive);
    }
    // Rule: StartDate >= now
    [Fact]
    public async Task Should_Have_Error_When_StartDate_In_The_Past()
    {
        var dto = new CreatePhaseDto
        {
            Title = "test",
            Description = "test",
            StartDate = DateTime.UtcNow.AddMinutes(-5) ,
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 100
        };
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.StartDateCanNotBeInPast);
    }
    // Rule: EndDate >= now
    [Fact]
    public async Task Should_Have_Error_When_EndDate_In_The_Past()
    {
        var dto = new CreatePhaseDto
        {
            Title = "test",
            Description ="test",
            StartDate = DateTime.UtcNow.AddMinutes(2) ,
            EndDate = DateTime.UtcNow.AddMinutes(-5),
            PhaseScore =100
        };
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.EndDateCanNotBeInPast);
    }
    // Rule: StartDate <= EndDate
    [Fact]
    public async Task Should_Fail_When_StartDate_After_EndDate()
    {
        var dto = new CreatePhaseDto
        {
            Title = "Phase 1",
            StartDate = DateTime.UtcNow.AddDays(5),
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 5
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.StartDateCanNotBeLaterThanEndDatte);
    }

    // Rule: PhaseFile -> invalid extension
    [Fact]
    public async Task Should_Have_Error_When_PhaseFile_Has_Invalid_Extension()
    {
        var fileMock = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("fake content")), 0, 10, "Data", "test.exe");
        _fileServiceMock.Setup(f => f.IsValidExtension(It.IsAny<IFormFile>())).Returns(false);

        var dto = new CreatePhaseDto
        {
            PhaseFile = fileMock,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 1,
            Title = "Valid"
        };
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.InvalidFormat);
    }

    // Rule: PhaseFile -> invalid size
    [Fact]
    public async Task Should_Have_Error_When_PhaseFile_Has_Invalid_Size()
    {
        var fileMock = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("fake content")), 0, 10, "Data", "test.pdf");
        _fileServiceMock.Setup(f => f.IsValidExtension(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(f => f.IsValidFileSize(It.IsAny<IFormFile>())).Returns(false);

        var dto = new CreatePhaseDto
        {
            PhaseFile = fileMock,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 1,
            Title = "Valid"
        };
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.InvalidSize);
    }

    // Rule: FileFormats -> invalid format
    [Fact]
    public async Task Should_Have_Error_When_FileFormats_Are_Invalid()
    {
        var dto = new CreatePhaseDto
        {
            FileFormats = "pdf,exe", // exe is invalid
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 1,
            Title = "Valid"
        };
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.InvalidFormatType);
    }

    
    [Fact]
    public async Task Should_Pass_Validation_When_Valid()
    {
        var dto = new CreatePhaseDto
        {
            Title = "Phase 1",
            Description = "Test",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 5,
            FileFormats = "pdf,zip"
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.True(result.IsValid);
    }


}
