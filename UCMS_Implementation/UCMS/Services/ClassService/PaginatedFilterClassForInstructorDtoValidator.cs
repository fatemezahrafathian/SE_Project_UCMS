using FluentValidation;
using UCMS.DTOs.ClassDto;
using UCMS.Resources;

namespace UCMS.Services.ClassService;

public class PaginatedFilterClassForInstructorDtoValidator : AbstractValidator<PaginatedFilterClassForInstructorDto>
{
    public PaginatedFilterClassForInstructorDtoValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Title)
            .MaximumLength(100).WithMessage(Messages.TitleMaxLength);

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage(Messages.PageMustBeGreaterThanZero);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage(Messages.PageSizeMustBeBetween1And100);
    }
}