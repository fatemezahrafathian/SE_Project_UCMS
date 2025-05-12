using FluentValidation;
using UCMS.DTOs.ClassDto;
using UCMS.Resources;
using UCMS.Services.ImageService;

namespace UCMS.Services.ClassService;

public class UpdateClassDtoValidator : AbstractValidator<PatchClassDto>
{
    public UpdateClassDtoValidator(IImageService imageService)
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;


        RuleFor(x => x.ProfileImage)
            .Must(file => file == null || imageService.IsValidImageExtension(file))
            .WithMessage(Messages.InvalidFormat);

        RuleFor(x => x.ProfileImage)
            .Must(file => file == null || imageService.IsValidImageSize(file))
            .WithMessage(Messages.InvalidSize);

        When(dto => dto.StartDate.HasValue && dto.EndDate.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(dto => dto.StartDate!.Value <= dto.EndDate!.Value)
                .WithMessage(Messages.StartDateCanNotBeLaterThanEndDatte);
        });
    }

    public void ApplyRuntimeRules(DateTime createdAt, DateTime updatedAt, DateOnly? previousStartDate, DateOnly? previousEndDate)
    {
        RuleFor(x => x.StartDate)
            .Must(date => !date.HasValue || date.Value >= DateOnly.FromDateTime(createdAt))
            .WithMessage(Messages.StartDateCanNotBeEarlierThanCreationTime);

        RuleFor(x => x.EndDate)
            .Must(date => !date.HasValue || date.Value >= DateOnly.FromDateTime(updatedAt))
            .WithMessage(Messages.EndDateCanNotBeEarlierThanUpdateTime);

        When(x => x.StartDate.HasValue && !x.EndDate.HasValue && previousEndDate.HasValue, () =>
        {
            RuleFor(x => x.StartDate!.Value)
                .LessThanOrEqualTo(_ => previousEndDate!.Value)
                .WithMessage(Messages.StartDateCanNotBeLaterThanEndDatte);
        });

        When(x => x.EndDate.HasValue && !x.StartDate.HasValue && previousStartDate.HasValue, () =>
        {
            RuleFor(x => x.EndDate!.Value)
                .GreaterThanOrEqualTo(_ => previousStartDate!.Value)
                .WithMessage(Messages.StartDateCanNotBeLaterThanEndDatte);
        });
    }
}
