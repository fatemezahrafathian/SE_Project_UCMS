using FluentValidation;
using UCMS.DTOs.PhaseDto;
using UCMS.Resources;
using UCMS.Services.FileService;

namespace UCMS.Services.PhaseService;

public class UpdatePhaseDtoValidator : AbstractValidator<PatchPhaseDto>
{
    private readonly List<string> _allowedFormats = new() { "pdf", "zip", "rar", "txt" };

    public UpdatePhaseDtoValidator(IFileService fileService)
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

        When(x => x.PhaseScore.HasValue, () =>
        {
            RuleFor(x => x.PhaseScore.Value)
                .GreaterThan(0).WithMessage(Messages.PhaseScoreMustBePositive);
        });

        // When(x => x.StartDate.HasValue, () =>
        // {
        //     RuleFor(x => x.StartDate.Value)
        //         .Must(date => date >= DateTime.UtcNow)
        //         .WithMessage(Messages.StartDateCanNotBeInPast);
        // });

        When(x => x.EndDate.HasValue, () =>
        {
            RuleFor(x => x.EndDate.Value)
                .Must(date => date >= DateTime.UtcNow)
                .WithMessage(Messages.EndDateCanNotBeInPast);
        });

        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(dto => dto.StartDate <= dto.EndDate)
                .WithMessage(Messages.StartDateCanNotBeLaterThanEndDatte);
        });

        When(x => x.PhaseFile != null, () =>
        {
            RuleFor(x => x.PhaseFile)
                .Must(file => fileService.IsValidExtension(file))
                .WithMessage(Messages.InvalidFormat);

            RuleFor(x => x.PhaseFile)
                .Must(file => fileService.IsValidFileSize(file))
                .WithMessage(Messages.InvalidSize);
        });

        When(x => !string.IsNullOrWhiteSpace(x.FileFormats), () =>
        {
            RuleFor(x => x.FileFormats)
                .Must(AllFormatsAreValid)
                .WithMessage(Messages.InvalidFormat);
        });
    }

    private bool AllFormatsAreValid(string? formats)
    {
        if (string.IsNullOrWhiteSpace(formats)) return true;

        var formatList = formats
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim().ToLower());

        return formatList.All(f => _allowedFormats.Contains(f));
    }
}
