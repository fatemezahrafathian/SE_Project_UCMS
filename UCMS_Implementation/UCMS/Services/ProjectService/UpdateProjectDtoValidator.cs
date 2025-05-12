using FluentValidation;
using UCMS.DTOs.ProjectDto;
using UCMS.Resources;
using UCMS.Services.FileService;

namespace UCMS.Services.ProjectService;

public class UpdateProjectDtoValidator : AbstractValidator<PatchProjectDto>
{
    public UpdateProjectDtoValidator(IFileService fileService)
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        When(x => x.Title != null, () =>
        {
            RuleFor(x => x.Title)
                .MaximumLength(100).WithMessage(Messages.TitleMaxLength);
        });

        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage(Messages.DescriptionMaxLength);
        });

        When(x => x.TotalScore.HasValue, () =>
        {
            RuleFor(x => x.TotalScore.Value)
                .GreaterThan(0).WithMessage(Messages.TotalScoreMustBePositive);
        });

        When(x => x.StartDate.HasValue, () =>
        {
            RuleFor(x => x.StartDate.Value)
                .Must(date => date >= DateTime.UtcNow)
                .WithMessage(Messages.StartDateCanNotBeInPast);
        });

        When(x => x.EndDate.HasValue, () =>
        {
            RuleFor(x => x.EndDate.Value)
                .Must(date => date >= DateTime.UtcNow)
                .WithMessage(Messages.EndDateCanNotBeInPast);
        });

        // StartDate <= EndDate (فقط وقتی هر دو مقدار داشته باشند)
        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(dto => dto.StartDate <= dto.EndDate)
                .WithMessage(Messages.StartDateCanNotBeLaterThanEndDatte);
        });

        When(x => x.ProjectFile != null, () =>
        {
            RuleFor(x => x.ProjectFile)
                .Must(file => fileService.IsValidExtension(file))
                .WithMessage(Messages.InvalidFormat);

            RuleFor(x => x.ProjectFile)
                .Must(file => fileService.IsValidFileSize(file))
                .WithMessage(Messages.InvalidSize);
        });
    }
}

