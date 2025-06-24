using FluentValidation;
using UCMS.DTOs.ClassDto;
using UCMS.Resources;

namespace UCMS.Services.ClassService;
public class UpdateClassEntriesDtoDtoValidator : AbstractValidator<UpdateClassEntriesDto>
{
    public UpdateClassEntriesDtoDtoValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;
        
        RuleFor(x => x.EntryDtos)
            .NotNull().WithMessage(Messages.EntryDtosMustNotBeNull);
            // .NotEmpty().WithMessage("Entries list must not be empty.");

        RuleForEach(x => x.EntryDtos).ChildRules(entry =>
        {
            // entry.RuleFor(e => e.EntryId)
            //     .GreaterThan(0).WithMessage("EntryId must be greater than 0.");

            entry.RuleFor(e => e.EntryType)
                .IsInEnum().WithMessage(Messages.InvalidEntryType);

            entry.RuleFor(e => e.PortionInTotalScore)
                .GreaterThan(0).WithMessage(Messages.InvalidPortionInTotalScore);
        });

        RuleFor(x => x.TotalScore)
            .GreaterThan(0).WithMessage(Messages.InvalidTotalScore);

        RuleFor(x => x)
            .Must(x => Math.Abs(x.EntryDtos.Sum(e => e.PortionInTotalScore) - x.TotalScore) < 0.001)
            .WithMessage(Messages.SumOfPortionInTotalScoreMustBeEqualToTotalScore);

    }
}