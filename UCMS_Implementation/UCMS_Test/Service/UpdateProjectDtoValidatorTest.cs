namespace UCMS_Test.Service;
using System;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Moq;
using UCMS.DTOs.ProjectDto;
using UCMS.Resources;
using UCMS.Services.FileService;
using UCMS.Services.ProjectService;
using Xunit;


public class UpdateProjectValidatorTest
{
    private readonly UpdateProjectDtoValidator _validator;
    private readonly Mock<IFileService> _fileServiceMock;

    public UpdateProjectValidatorTest()
    {
        _fileServiceMock = new Mock<IFileService>();
        _fileServiceMock.Setup(x => x.IsValidExtension(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(x => x.IsValidFileSize(It.IsAny<IFormFile>())).Returns(true);

        _validator = new UpdateProjectDtoValidator(_fileServiceMock.Object);
    }

    private PatchProjectDto GetValidDto() => new PatchProjectDto
    {
        Title = "Valid Title",
        Description = "Valid Description",
        TotalScore = 100,
        StartDate = DateTime.UtcNow.AddDays(1),
        EndDate = DateTime.UtcNow.AddDays(2)
    };

    [Fact]
    public void ValidDto_Should_Pass()
    {
        var result = _validator.TestValidate(GetValidDto());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void TitleTooLong_Should_Fail()
    {
        var dto = GetValidDto();
        dto.Title = new string('A', 101);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage(Messages.TitleMaxLength);
    }

    [Fact]
    public void DescriptionTooLong_Should_Fail()
    {
        var dto = GetValidDto();
        dto.Description = new string('B', 501);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Description)
              .WithErrorMessage(Messages.DescriptionMaxLength);
    }

    [Fact]
    public void TotalScore_Negative_Should_Fail()
    {
        var dto = GetValidDto();
        dto.TotalScore = -10;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.TotalScore.Value)
              .WithErrorMessage(Messages.TotalScoreMustBePositive);
    }

    [Fact]
    public void StartDate_In_The_Past_Should_Fail()
    {
        var dto = GetValidDto();
        dto.StartDate = DateTime.UtcNow.AddDays(-1);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.StartDate.Value)
              .WithErrorMessage(Messages.StartDateCanNotBeInPast);
    }

    [Fact]
    public void EndDate_In_The_Past_Should_Fail()
    {
        var dto = GetValidDto();
        dto.EndDate = DateTime.UtcNow.AddDays(-1);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.EndDate.Value)
              .WithErrorMessage(Messages.EndDateCanNotBeInPast);
    }

    [Fact]
    public void StartDate_After_EndDate_Should_Fail()
    {
        var dto = GetValidDto();
        dto.StartDate = DateTime.UtcNow.AddDays(5);
        dto.EndDate = DateTime.UtcNow.AddDays(2);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x)
              .WithErrorMessage(Messages.StartDateCanNotBeLaterThanEndDatte);
    }

    [Fact]
    public void Invalid_File_Extension_Should_Fail()
    {
        var dto = GetValidDto();
        dto.ProjectFile = Mock.Of<IFormFile>();
        _fileServiceMock.Setup(x => x.IsValidExtension(dto.ProjectFile)).Returns(false);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ProjectFile)
              .WithErrorMessage(Messages.InvalidFormat);
    }

    [Fact]
    public void Invalid_File_Size_Should_Fail()
    {
        var dto = GetValidDto();
        dto.ProjectFile = Mock.Of<IFormFile>();
        _fileServiceMock.Setup(x => x.IsValidFileSize(dto.ProjectFile)).Returns(false);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ProjectFile)
              .WithErrorMessage(Messages.InvalidSize);
    }

    [Fact]
    public void NullFields_Should_Not_Trigger_Validation()
    {
        var dto = new PatchProjectDto(); // همه فیلدها null
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
