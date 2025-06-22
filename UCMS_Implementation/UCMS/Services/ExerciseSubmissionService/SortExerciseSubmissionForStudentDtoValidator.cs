using FluentValidation;
using UCMS.DTOs.ExerciseSubmissionDto;
using UCMS.DTOs.PhaseSubmissionDto;
using UCMS.Resources;

namespace UCMS.Services.ExerciseSubmissionService;

public class SortExerciseSubmissionForStudentDtoValidator : AbstractValidator<SortExerciseSubmissionsStudentDto> // move this from repository to service
{
    public SortExerciseSubmissionForStudentDtoValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        When(x => x.SortBy != SortExerciseSubmissionByForStudentOption.None, () =>
        {
            RuleFor(x => x.SortOrder)
                .Must(order => order == SortOrderOption.Ascending || order == SortOrderOption.Descending)
                .WithMessage(Messages.SortOrderOptionCanNotBeNone);
        });

        When(x => x.SortBy == SortExerciseSubmissionByForStudentOption.None, () =>
        {
            RuleFor(x => x.SortOrder)
                .Equal(SortOrderOption.None)
                .WithMessage(Messages.SortOrderOptionMustBeNone);
        });
    }
    
}