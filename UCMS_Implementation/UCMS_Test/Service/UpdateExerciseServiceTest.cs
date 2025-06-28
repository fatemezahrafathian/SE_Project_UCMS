using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Moq;
using UCMS.DTOs.ExerciseDto;
using UCMS.Resources;
using UCMS.Services.ExerciseService;
using UCMS.Services.FileService;

namespace UCMS_Test.Service;

public class UpdateExerciseServiceTest
{
    private readonly UpdateExerciseDtoValidator _validator;
    private readonly Mock<IFileService> _fileServiceMock;

    public UpdateExerciseServiceTest()
    {
        _fileServiceMock = new Mock<IFileService>();
        _fileServiceMock.Setup(x => x.IsValidExtension(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(x => x.IsValidFileSize(It.IsAny<IFormFile>())).Returns(true);

        _validator = new UpdateExerciseDtoValidator(_fileServiceMock.Object);
    }

    private PatchExerciseDto GetValidDto() => new PatchExerciseDto
    {
        Title = "Valid Title",
        Description = "Valid Description",
        ExerciseScore = 10,
        StartDate = DateTime.UtcNow.AddDays(1),
        EndDate = DateTime.UtcNow.AddDays(2),
        FileFormats = "pdf, zip"
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
    public void NegativeExerciseScore_Should_Fail()
    {
        var dto = GetValidDto();
        dto.ExerciseScore = -1;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ExerciseScore.Value)
              .WithErrorMessage(Messages.ExerciseScoreMustBePositive);
    }

    // [Fact]
    // public void StartDateInPast_Should_Fail()
    // {
    //     var dto = GetValidDto();
    //     dto.StartDate = DateTime.UtcNow.AddDays(-1);
    //     var result = _validator.TestValidate(dto);
    //     result.ShouldHaveValidationErrorFor(x => x.StartDate.Value)
    //           .WithErrorMessage(Messages.StartDateCanNotBeInPast);
    // }

    [Fact]
    public void EndDateInPast_Should_Fail()
    {
        var dto = GetValidDto();
        dto.EndDate = DateTime.UtcNow.AddDays(-1);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.EndDate.Value)
              .WithErrorMessage(Messages.EndDateCanNotBeInPast);
    }

    [Fact]
    public void StartDateAfterEndDate_Should_Fail()
    {
        var dto = GetValidDto();
        dto.StartDate = DateTime.UtcNow.AddDays(5);
        dto.EndDate = DateTime.UtcNow.AddDays(2);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x)
              .WithErrorMessage(Messages.StartDateCanNotBeLaterThanEndDatte);
    }

    [Fact]
    public void InvalidFileExtension_Should_Fail()
    {
        _fileServiceMock.Setup(x => x.IsValidExtension(It.IsAny<IFormFile>())).Returns(false);
        var dto = GetValidDto();
        dto.ExerciseFile = Mock.Of<IFormFile>();
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ExerciseFile)
              .WithErrorMessage(Messages.InvalidFormat);
    }

    [Fact]
    public void InvalidFileSize_Should_Fail()
    {
        _fileServiceMock.Setup(x => x.IsValidFileSize(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(x => x.IsValidFileSize(It.IsAny<IFormFile>())).Returns(false);
        var dto = GetValidDto();
        dto.ExerciseFile = Mock.Of<IFormFile>();
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ExerciseFile)
              .WithErrorMessage(Messages.InvalidSize);
    }

    [Fact]
    public void InvalidFileFormats_Should_Fail()
    {
        var dto = GetValidDto();
        dto.FileFormats = "pdf, exe";
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.FileFormats)
              .WithErrorMessage(Messages.InvalidFormat);
    }
}
