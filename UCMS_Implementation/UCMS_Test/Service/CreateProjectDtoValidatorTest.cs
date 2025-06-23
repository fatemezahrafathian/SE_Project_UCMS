namespace UCMS_Test.Service;
using System.Text;
using Microsoft.AspNetCore.Http;
using Moq;
using UCMS.DTOs.ProjectDto;
using UCMS.Models;
using UCMS.Resources;
using UCMS.Services.FileService;
using UCMS.Services.ProjectService;


public class CreateProjectValidatorTest
{
    private readonly CreateProjectDtoValidator _validator;
    private readonly Mock<IFileService> _fileServiceMock;

    public CreateProjectValidatorTest()
    {
        _fileServiceMock = new Mock<IFileService>();
        _fileServiceMock.Setup(x => x.IsValidExtension(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(x => x.IsValidFileSize(It.IsAny<IFormFile>())).Returns(true);

        _validator = new CreateProjectDtoValidator(_fileServiceMock.Object);
    }

    [Fact]
    public async Task Should_Have_Error_When_Title_Is_Empty()
    {
        var dto = new CreateProjectDto { Title = "", TotalScore = 10, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(2), ProjectType = 0 };
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.TitleIsRequired);
    }

    [Fact]
    public async Task Should_Have_Error_When_Title_Too_Long()
    {
        var dto = new CreateProjectDto { Title = new string('a', 101), TotalScore = 10, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(2), ProjectType = 0 };
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.TitleMaxLength);
    }

    [Fact]
    public async Task Should_Have_Error_When_Description_Too_Long()
    {
        var dto = new CreateProjectDto { Title = "Valid", Description = new string('a', 501), TotalScore = 10, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(2), ProjectType = 0 };
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.DescriptionMaxLength);
    }

    [Fact]
    public async Task Should_Have_Error_When_TotalScore_Not_Positive()
    {
        var dto = new CreateProjectDto { Title = "Valid", TotalScore = 0, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(2), ProjectType = 0 };
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.TotalScoreMustBePositive);
    }

    [Fact]
    public async Task Should_Have_Error_When_StartDate_In_The_Past()
    {
        var dto = new CreateProjectDto { Title = "Valid", TotalScore = 10, StartDate = DateTime.UtcNow.AddMinutes(-1), EndDate = DateTime.UtcNow.AddDays(2), ProjectType = 0 };
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.StartDateCanNotBeInPast);
    }

    [Fact]
    public async Task Should_Have_Error_When_EndDate_In_The_Past()
    {
        var dto = new CreateProjectDto { Title = "Valid", TotalScore = 10, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddMinutes(-1), ProjectType = 0 };
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.EndDateCanNotBeInPast);
    }

    [Fact]
    public async Task Should_Have_Error_When_StartDate_Is_After_EndDate()
    {
        var dto = new CreateProjectDto { Title = "Valid", TotalScore = 10, StartDate = DateTime.UtcNow.AddDays(5), EndDate = DateTime.UtcNow.AddDays(2), ProjectType = 0 };
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.StartDateCanNotBeLaterThanEndDatte);
    }

    [Fact]
    public async Task Should_Have_Error_When_ProjectFile_Has_Invalid_Extension()
    {
        var fileMock = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("fake content")), 0, 10, "Data", "test.exe");
        _fileServiceMock.Setup(x => x.IsValidExtension(It.IsAny<IFormFile>())).Returns(false);

        var dto = new CreateProjectDto { Title = "Valid", TotalScore = 10, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(2), ProjectType = 0, ProjectFile = fileMock };
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.InvalidFormat);
    }

    [Fact]
    public async Task Should_Have_Error_When_ProjectFile_Has_Invalid_Size()
    {
        var fileMock = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("fake content")), 0, 10, "Data", "test.pdf");
        _fileServiceMock.Setup(x => x.IsValidFileSize(It.IsAny<IFormFile>())).Returns(false);

        var dto = new CreateProjectDto { Title = "Valid", TotalScore = 10, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(2), ProjectType = 0, ProjectFile = fileMock };
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.InvalidSize);
    }

    [Fact]
    public async Task Should_Have_Error_When_ProjectType_Is_Invalid()
    {
        var dto = new CreateProjectDto { Title = "Valid", TotalScore = 10, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(2), ProjectType = 2 };
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.InvalidProjectTypeSelected);
    }

    [Fact]
    public async Task Should_Have_Error_When_GroupSize_Is_Invalid_For_Group_Project()
    {
        var dto = new CreateProjectDto { Title = "Valid", TotalScore = 10, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(2), ProjectType = (int)ProjectType.Group, GroupSize = 1 };
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.InvalidGroupSize);
    }

    [Fact]
    public async Task Should_Have_Error_When_GroupSize_Is_Invalid_For_Individual_Project()
    {
        var dto = new CreateProjectDto { Title = "Valid", TotalScore = 10, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(2), ProjectType = (int)ProjectType.Individual, GroupSize = 3 };
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == Messages.InvalidGroupSize);
    }

    [Fact]
    public async Task Should_Pass_Validation_When_All_Is_Valid()
    {
        var dto = new CreateProjectDto
        {
            Title = "Valid",
            TotalScore = 100,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            ProjectType = (int)ProjectType.Group,
            GroupSize = 3
        };

        var result = await _validator.ValidateAsync(dto);
        Assert.True(result.IsValid);
    }
}
