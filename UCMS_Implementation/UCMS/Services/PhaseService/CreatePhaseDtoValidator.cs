using FluentValidation;
using UCMS.Models;
using UCMS.Resources;
using UCMS.Services.FileService;
using FluentValidation;
using UCMS.DTOs.PhaseDto;
using UCMS.DTOs.ProjectDto;
using UCMS.Models;
using UCMS.Resources;
using UCMS.Services.FileService;

namespace UCMS.Services.PhaseService;

public class CreatePhaseDtoValidator: AbstractValidator<CreatePhaseDto>
{
    private readonly List<string> _allowedFormats = new() { "pdf", "zip", "rar", "txt" };
    public CreatePhaseDtoValidator(IFileService fileService)
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(Messages.TitleIsRequired)
            .MaximumLength(100).WithMessage(Messages.TitleMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(Messages.DescriptionMaxLength);

        RuleFor(x => x.PhaseScore)
            .GreaterThan(0).WithMessage(Messages.PhaseScoreMustBePositive);

        RuleFor(x => x.StartDate)
            .Must(date => date >= DateTime.UtcNow)
            .WithMessage(Messages.StartDateCanNotBeInPast);

        RuleFor(x => x.EndDate)
            .Must(date => date >= DateTime.UtcNow)
            .WithMessage(Messages.EndDateCanNotBeInPast);

        RuleFor(x => x)
            .Must(dto => dto.StartDate <= dto.EndDate)
            .WithMessage(Messages.StartDateCanNotBeLaterThanEndDatte);

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