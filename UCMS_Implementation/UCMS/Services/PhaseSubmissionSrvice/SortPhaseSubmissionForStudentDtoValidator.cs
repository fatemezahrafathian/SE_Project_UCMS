using FluentValidation;
using UCMS.DTOs.ExerciseSubmissionDto;
using UCMS.DTOs.PhaseSubmissionDto;
using UCMS.Resources;

namespace UCMS.Services.PhaseSubmissionSrvice;

public class SortPhaseSubmissionForStudentDtoValidator : AbstractValidator<SortPhaseSubmissionsStudentDto> // move this from repository to service
{
    public SortPhaseSubmissionForStudentDtoValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        When(x => x.SortBy != SortPhaseSubmissionByForStudentOption.None, () =>
        {
            RuleFor(x => x.SortOrder)
                .Must(order => order == SortOrderOption.Ascending || order == SortOrderOption.Descending)
                .WithMessage(Messages.SortOrderOptionCanNotBeNone);
        });

        When(x => x.SortBy == SortPhaseSubmissionByForStudentOption.None, () =>
        {
            RuleFor(x => x.SortOrder)
                .Equal(SortOrderOption.None)
                .WithMessage(Messages.SortOrderOptionMustBeNone);
        });
    }
    
}