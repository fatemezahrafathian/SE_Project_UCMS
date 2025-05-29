using FluentValidation;
using UCMS.DTOs.ExamDto;
using UCMS.Resources;

namespace UCMS.Services.ExamService;

public class UpdateExamDtoValidator:AbstractValidator<PatchExamDto>
{
    public UpdateExamDtoValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        When(x => x.Title != null, () =>
        {
            RuleFor(x => x.Title)
                .MaximumLength(100).WithMessage(Messages.TitleMaxLength);
        });

        When(x => x.ExamLocation != null, () =>
        {
            RuleFor(x => x.ExamLocation)
                .MaximumLength(500).WithMessage(Messages.DescriptionMaxLength);
        });

        When(x => x.ExamScore.HasValue, () =>
        {
            RuleFor(x => x.ExamScore.Value)
                .GreaterThan(0).WithMessage(Messages.ExamScoreMustBePositive);
        });

        When(x => x.Date.HasValue, () =>
        {
            RuleFor(x => x.Date.Value)
                .Must(date => date >= DateTime.UtcNow)
                .WithMessage(Messages.DateCanNotBeInPast);
        });
    }
}
