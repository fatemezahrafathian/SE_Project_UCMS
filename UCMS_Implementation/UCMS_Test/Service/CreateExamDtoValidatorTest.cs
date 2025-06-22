namespace UCMS_Test.Service;

using UCMS.DTOs.ExamDto;
using UCMS.Resources;
using UCMS.Services.ExamService;
using FluentValidation.TestHelper;


public class CreateExamValidatorTest
{
    private readonly CreateExamDtoValidator _validator;

    public CreateExamValidatorTest()
    {
        _validator = new CreateExamDtoValidator();
    }

    private CreateExamDto GetValidDto() => new CreateExamDto
    {
        Title = "Valid Exam Title",
        ExamLocation = "Room 101, Main Building",
        ExamScore = 20,
        Date = DateTime.UtcNow.AddDays(1)
    };

    [Fact]
    public void Should_Pass_When_Valid()
    {
        var result = _validator.TestValidate(GetValidDto());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Title_Is_Empty()
    {
        var dto = GetValidDto();
        dto.Title = "";
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage(Messages.TitleIsRequired);
    }

    [Fact]
    public void Should_Fail_When_Title_Too_Long()
    {
        var dto = GetValidDto();
        dto.Title = new string('A', 101); // بیش از 100 کاراکتر
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage(Messages.TitleMaxLength);
    }

    [Fact]
    public void Should_Fail_When_ExamLocation_Too_Long()
    {
        var dto = GetValidDto();
        dto.ExamLocation = new string('B', 501); // بیش از 500 کاراکتر
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ExamLocation)
              .WithErrorMessage(Messages.ExamLocationMaxLength);
    }

    [Fact]
    public void Should_Fail_When_ExamScore_Is_Zero_Or_Less()
    {
        var dto = GetValidDto();
        dto.ExamScore = 0;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ExamScore)
              .WithErrorMessage(Messages.ExamScoreMustBePositive);

        dto.ExamScore = -5;
        result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ExamScore)
              .WithErrorMessage(Messages.ExamScoreMustBePositive);
    }

    [Fact]
    public void Should_Fail_When_Date_Is_In_The_Past()
    {
        var dto = GetValidDto();
        dto.Date = DateTime.UtcNow.AddMinutes(-10); // تاریخ گذشته
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Date)
              .WithErrorMessage(Messages.DateCanNotBeInPast);
    }
}
