using FluentValidation;
using UCMS.DTOs.PhaseSubmissionDto;
using UCMS.Resources;

namespace UCMS.Services.PhaseSubmissionSrvice;

public class SortPhaseSubmissionForInstructorDtoValidator
    : AbstractValidator<SortPhaseSubmissionsForInstructorDto>
{
    public SortPhaseSubmissionForInstructorDtoValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        // index non negative

        When(x => x.SortBy != SortPhaseSubmissionByForInstructorOption.None, () =>
        {
            RuleFor(x => x.SortOrder)
                .Must(order => order == SortOrderOption.Ascending || order == SortOrderOption.Descending)
                .WithMessage(Messages.SortOrderOptionCanNotBeNone);
        });

        When(x => x.SortBy == SortPhaseSubmissionByForInstructorOption.None, () =>
        {
            RuleFor(x => x.SortOrder)
                .Equal(SortOrderOption.None)
                .WithMessage(Messages.SortOrderOptionMustBeNone);
        });
    }
}