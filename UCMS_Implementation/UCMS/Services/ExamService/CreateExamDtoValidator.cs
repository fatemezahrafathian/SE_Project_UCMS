using FluentValidation;
using UCMS.DTOs.ExamDto;
using UCMS.Resources;
using UCMS.Services.FileService;

namespace UCMS.Services.ExamService;

public class CreateExamDtoValidator: AbstractValidator<CreateExamDto>
{
    public CreateExamDtoValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(Messages.TitleIsRequired)
            .MaximumLength(100).WithMessage(Messages.TitleMaxLength);

        RuleFor(x => x.ExamLocation)
            .MaximumLength(500).WithMessage(Messages.ExamLocationMaxLength);

        RuleFor(x => x.ExamScore)
            .GreaterThan(0).WithMessage(Messages.ExamScoreMustBePositive);

        RuleFor(x => x.Date)
            .Must(date => date >= DateTime.UtcNow)
            .WithMessage(Messages.DateCanNotBeInPast);
        
    }
}