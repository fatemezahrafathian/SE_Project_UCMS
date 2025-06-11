using FluentValidation;
using UCMS.DTOs.TeamPhaseDto;
using UCMS.Resources;

namespace UCMS.Repositories.PhaseSubmissionRepository;

public class CreatePhaseSubmissionDtoValidator : AbstractValidator<CreatePhaseSubmissionDto> // move this from repository to service
{
    public CreatePhaseSubmissionDtoValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.SubmissionFile)
            .NotNull().WithMessage(Messages.FileIsNeeded);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage(Messages.DescriptionMaxLength);
    }
    
}