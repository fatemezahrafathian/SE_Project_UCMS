using Microsoft.AspNetCore.Http;

namespace UCMS_Test.Service;
using System;
using FluentValidation.TestHelper;
using Moq;
using UCMS.DTOs.PhaseDto;
using UCMS.Services.FileService;
using UCMS.Services.PhaseService;
using Xunit;

public class UpdatePhaseValidatorTest
{
    private readonly UpdatePhaseDtoValidator _validator;
    private readonly Mock<IFileService> _fileServiceMock;

    public UpdatePhaseValidatorTest()
    {
        _fileServiceMock = new Mock<IFileService>();
        _fileServiceMock.Setup(x => x.IsValidExtension(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(x => x.IsValidFileSize(It.IsAny<IFormFile>())).Returns(true);

        _validator = new UpdatePhaseDtoValidator(_fileServiceMock.Object);
    }

    private PatchPhaseDto GetValidDto() => new PatchPhaseDto
    {
        Title = "Valid Title",
        Description = "Valid Description",
        PhaseScore = 10,
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
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void DescriptionTooLong_Should_Fail()
    {
        var dto = GetValidDto();
        dto.Description = new string('B', 501);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void NegativePhaseScore_Should_Fail()
    {
        var dto = GetValidDto();
        dto.PhaseScore = -1;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x=>x.PhaseScore.Value);
    }

    // [Fact]
    // public void StartDateInPast_Should_Fail()
    // {
    //     var dto = GetValidDto();
    //     dto.StartDate = DateTime.UtcNow.AddDays(-1);
    //     var result = _validator.TestValidate(dto);
    //     result.ShouldHaveValidationErrorFor(x=>x.StartDate.Value);
    // }

    [Fact]
    public void EndDateInPast_Should_Fail()
    {
        var dto = GetValidDto();
        dto.EndDate = DateTime.UtcNow.AddDays(-1);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.EndDate.Value); 
    }


    [Fact]
    public void StartDateAfterEndDate_Should_Fail()
    {
        var dto = GetValidDto();
        dto.StartDate = DateTime.UtcNow.AddDays(5);
        dto.EndDate = DateTime.UtcNow.AddDays(2);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void InvalidFileExtension_Should_Fail()
    {
        _fileServiceMock.Setup(x => x.IsValidExtension(It.IsAny<IFormFile>())).Returns(false);
        var dto = GetValidDto();
        dto.PhaseFile = Mock.Of<IFormFile>();
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.PhaseFile);
    }

    [Fact]
    public void InvalidFileSize_Should_Fail()
    {
        _fileServiceMock.Setup(x => x.IsValidFileSize(It.IsAny<IFormFile>())).Returns(false);
        var dto = GetValidDto();
        dto.PhaseFile = Mock.Of<IFormFile>();
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.PhaseFile);
    }

    [Fact]
    public void InvalidFileFormats_Should_Fail()
    {
        var dto = GetValidDto();
        dto.FileFormats = "pdf, exe";
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.FileFormats);
    }
}
