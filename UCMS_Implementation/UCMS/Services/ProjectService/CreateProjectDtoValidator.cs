using FluentValidation;
using UCMS.DTOs.ProjectDto;
using UCMS.Models;
using UCMS.Resources;
using UCMS.Services.FileService;

namespace UCMS.Services.ProjectService;
public class CreateProjectDtoValidator : AbstractValidator<CreateProjectDto>
{
    public CreateProjectDtoValidator(IFileService fileService)
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(Messages.TitleIsRequired)
            .MaximumLength(100).WithMessage(Messages.TitleMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(Messages.DescriptionMaxLength);

        RuleFor(x => x.TotalScore)
            .GreaterThan(0).WithMessage(Messages.TotalScoreMustBePositive);

        RuleFor(x => x.StartDate)
            .Must(date => date >= DateTime.UtcNow)
            .WithMessage(Messages.StartDateCanNotBeInPast);

        RuleFor(x => x.EndDate)
            .Must(date => date >= DateTime.UtcNow)
            .WithMessage(Messages.EndDateCanNotBeInPast);

        RuleFor(x => x)
            .Must(dto => dto.StartDate <= dto.EndDate)
            .WithMessage(Messages.StartDateCanNotBeLaterThanEndDatte);

        When(x => x.ProjectFile != null, () =>
        {
            RuleFor(x => x.ProjectFile)
                .Must(file => fileService.IsValidExtension(file))
                .WithMessage(Messages.InvalidFormat);

            RuleFor(x => x.ProjectFile)
                .Must(file => fileService.IsValidFileSize(file))
                .WithMessage(Messages.InvalidSize);
        });
        RuleFor(x => x.ProjectType)
            .Must(value => value == 0 || value == 1)
            .WithMessage(Messages.InvalidProjectTypeSelected);

        When(x => x.ProjectType == (int)ProjectType.Group, () =>
        {
            RuleFor(x => x.GroupSize)
                .NotNull().WithMessage(Messages.InvalidGroupSize)
                .GreaterThan(1).WithMessage(Messages.InvalidGroupSize);
        });

        When(x => x.ProjectType == (int)ProjectType.Individual, () =>
        {
            RuleFor(x => x.GroupSize)
                .Must(size => size == null || size == 1)
                .WithMessage(Messages.InvalidGroupSize);
        });

    }
}