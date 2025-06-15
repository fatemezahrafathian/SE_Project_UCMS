using FluentValidation;
using UCMS.DTOs.ExerciseSubmissionDto;
using UCMS.Resources;

namespace UCMS.Services.ExerciseSubmissionService;

public class CreateExerciseSubmissionDtoValidator : AbstractValidator<CreateExerciseSubmissionDto> // move this from repository to service
{
    public CreateExerciseSubmissionDtoValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.SubmissionFile)
            .NotNull().WithMessage(Messages.FileIsNeeded);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage(Messages.DescriptionMaxLength);
    }
    
}