namespace UCMS_Test.Service;


using FluentValidation.TestHelper;
using UCMS.DTOs.ExamDto;
using UCMS.Resources;
using UCMS.Services.ExamService;

public class UpdateExamDtoValidatorTests
{
    private readonly UpdateExamDtoValidator _validator;

    public UpdateExamDtoValidatorTests()
    {
        _validator = new UpdateExamDtoValidator();
    }

    private PatchExamDto GetValidDto() => new PatchExamDto
    {
        Title = "Sample Exam",
        ExamLocation = "Main Building, Room 101",
        ExamScore = 20,
        Date = DateTime.UtcNow.AddDays(1)
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
        dto.Title = new string('A', 101); // بیش از 100 کاراکتر
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage(Messages.TitleMaxLength);
    }

    [Fact]
    public void ExamLocationTooLong_Should_Fail()
    {
        var dto = GetValidDto();
        dto.ExamLocation = new string('B', 501); // بیش از 500 کاراکتر
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ExamLocation)
              .WithErrorMessage(Messages.DescriptionMaxLength);
    }

    [Fact]
    public void NegativeExamScore_Should_Fail()
    {
        var dto = GetValidDto();
        dto.ExamScore = -5;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ExamScore.Value)
              .WithErrorMessage(Messages.ExamScoreMustBePositive);
    }

    [Fact]
    public void ZeroExamScore_Should_Fail()
    {
        var dto = GetValidDto();
        dto.ExamScore = 0;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ExamScore.Value)
              .WithErrorMessage(Messages.ExamScoreMustBePositive);
    }

    [Fact]
    public void DateInPast_Should_Fail()
    {
        var dto = GetValidDto();
        dto.Date = DateTime.UtcNow.AddDays(-1);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Date.Value)
              .WithErrorMessage(Messages.DateCanNotBeInPast);
    }

    [Fact]
    public void NullProperties_Should_Skip_Validation()
    {
        var dto = new PatchExamDto(); // همه‌ی مقادیر null
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors(); // هیچ قانونی اعمال نمی‌شود چون همه شرط‌ها null هستند
    }
}
