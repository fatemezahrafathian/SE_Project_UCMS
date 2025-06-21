using System.Text;
using Microsoft.AspNetCore.Http;
using Moq;
using UCMS.DTOs.ExerciseDto;
using UCMS.Resources;
using UCMS.Services.ExerciseService;
using UCMS.Services.FileService;

namespace UCMS_Test.Service;

public class CreateExerciseValidatorTest
{
    private readonly CreateExerciseDtoValidator _validator;
    private readonly Mock<IFileService> _fileServiceMock;

    public CreateExerciseValidatorTest()
    {
        _fileServiceMock = new Mock<IFileService>();
        _fileServiceMock.Setup(x => x.IsValidExtension(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(x => x.IsValidFileSize(It.IsAny<IFormFile>())).Returns(true);

        _validator = new CreateExerciseDtoValidator(_fileServiceMock.Object);
    }

    private CreateExerciseDto GetValidDto() => new CreateExerciseDto
    {
        Title = "Valid Title",
        Description = "Valid Description",
        ExerciseScore = 10,
        StartDate = DateTime.UtcNow.AddDays(1),
        EndDate = DateTime.UtcNow.AddDays(2),
        FileFormats = "pdf, zip"
    };

    [Fact]
    public async Task Should_Have_Error_When_Title_Is_Empty()
    {
        var dto = GetValidDto();
        dto.Title = "";
        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == Messages.TitleIsRequired);
    }

    [Fact]
    public async Task Should_Have_Error_When_Title_Too_Long()
    {
        var dto = GetValidDto();
        dto.Title = new string('a', 101);
        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == Messages.TitleMaxLength);
    }

    [Fact]
    public async Task Should_Have_Error_When_Description_Too_Long()
    {
        var dto = GetValidDto();
        dto.Description = new string('a', 501);
        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == Messages.DescriptionMaxLength);
    }

    [Fact]
    public async Task Should_Have_Error_When_ExerciseScore_Is_Zero_Or_Less()
    {
        var dto = GetValidDto();
        dto.ExerciseScore = 0;
        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == Messages.ExerciseScoreMustBePositive);
    }

    [Fact]
    public async Task Should_Have_Error_When_StartDate_In_The_Past()
    {
        var dto = GetValidDto();
        dto.StartDate = DateTime.UtcNow.AddMinutes(-5);
        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == Messages.StartDateCanNotBeInPast);
    }

    [Fact]
    public async Task Should_Have_Error_When_EndDate_In_The_Past()
    {
        var dto = GetValidDto();
        dto.EndDate = DateTime.UtcNow.AddMinutes(-5);
        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == Messages.EndDateCanNotBeInPast);
    }

    [Fact]
    public async Task Should_Fail_When_StartDate_After_EndDate()
    {
        var dto = GetValidDto();
        dto.StartDate = DateTime.UtcNow.AddDays(5);
        dto.EndDate = DateTime.UtcNow.AddDays(2);

        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == Messages.StartDateCanNotBeLaterThanEndDatte);
    }

    [Fact]
    public async Task Should_Have_Error_When_ExerciseFile_Has_Invalid_Extension()
    {
        var fileMock = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("fake content")), 0, 10, "Data", "test.exe");
        _fileServiceMock.Setup(f => f.IsValidExtension(It.IsAny<IFormFile>())).Returns(false);

        var dto = GetValidDto();
        dto.ExerciseFile = fileMock;

        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == Messages.InvalidFormat);
    }

    [Fact]
    public async Task Should_Have_Error_When_ExerciseFile_Has_Invalid_Size()
    {
        var fileMock = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("fake content")), 0, 10, "Data", "test.pdf");
        _fileServiceMock.Setup(f => f.IsValidExtension(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(f => f.IsValidFileSize(It.IsAny<IFormFile>())).Returns(false);

        var dto = GetValidDto();
        dto.ExerciseFile = fileMock;

        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == Messages.InvalidSize);
    }

    [Fact]
    public async Task Should_Have_Error_When_FileFormats_Are_Invalid()
    {
        var dto = GetValidDto();
        dto.FileFormats = "pdf,exe";

        var result = await _validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == Messages.InvalidFormat);
    }

    [Fact]
    public async Task Should_Pass_Validation_When_Valid()
    {
        var dto = GetValidDto();

        var result = await _validator.ValidateAsync(dto);

        Assert.True(result.IsValid);
    }
}
