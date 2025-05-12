using FluentValidation;
using UCMS.DTOs.ClassDto;
using UCMS.Resources;
using UCMS.Services.ImageService;

namespace UCMS.Services.ClassService;

public class CreateClassDtoValidator : AbstractValidator<CreateClassDto>
{
    public CreateClassDtoValidator(IImageService imageService)
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(Messages.TitleIsRequired)
            .MaximumLength(100).WithMessage(Messages.TitleMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(Messages.DescriptionMaxLength);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(Messages.PasswordIsRequired)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$") // se password service functions or implement here
            .WithMessage(Messages.PasswordNotStrong);

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage(Messages.ConfirmPasswordIsRequired)
            .Equal(x => x.Password).WithMessage(Messages.PasswordsDoNotMatch);

        RuleFor(x => x.Schedules)
            .NotNull().WithMessage(Messages.SchedulesRequired);
            // .Must(schedules => schedules != null && schedules.Count > 0)
            // .WithMessage(Messages.SchedulesAtLeastOne);

        RuleFor(x => x.StartDate)
            .Must(date => !date.HasValue || date.Value >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage(Messages.StartDateCanNotBeInPast);

        RuleFor(x => x.EndDate)
            .Must(date => !date.HasValue || date.Value >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage(Messages.EndDateCanNotBeInPast);

        RuleFor(x => x)
            .Must(dto => !dto.StartDate.HasValue || !dto.EndDate.HasValue || dto.StartDate.Value <= dto.EndDate.Value)
            .WithMessage(Messages.StartDateCanNotBeLaterThanEndDatte);

        RuleFor(x => x.ProfileImage)
            .Must(file => file == null || imageService.IsValidImageExtension(file))
            .WithMessage(Messages.InvalidFormat);

        RuleFor(x => x.ProfileImage)
            .Must(file => file == null || imageService.IsValidImageSize(file))
            .WithMessage(Messages.InvalidSize);
    }
}