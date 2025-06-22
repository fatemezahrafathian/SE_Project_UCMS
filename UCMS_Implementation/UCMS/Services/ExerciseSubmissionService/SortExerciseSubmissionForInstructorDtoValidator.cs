using FluentValidation;
using UCMS.DTOs.ExerciseSubmissionDto;
using UCMS.Resources;
using UCMS.DTOs.PhaseSubmissionDto;

namespace UCMS.Services.ExerciseSubmissionService;

public class SortExerciseSubmissionForInstructorDtoValidator 
    : AbstractValidator<SortExerciseSubmissionsForInstructorDto>
{
    public SortExerciseSubmissionForInstructorDtoValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        // index non negative

        When(x => x.SortBy != SortExerciseSubmissionByForInstructorOption.None, () =>
        {
            RuleFor(x => x.SortOrder)
                .Must(order => order == SortOrderOption.Ascending || order == SortOrderOption.Descending)
                .WithMessage(Messages.SortOrderOptionCanNotBeNone);
        });

        When(x => x.SortBy == SortExerciseSubmissionByForInstructorOption.None, () =>
        {
            RuleFor(x => x.SortOrder)
                .Equal(SortOrderOption.None)
                .WithMessage(Messages.SortOrderOptionMustBeNone);
        });
    }
}