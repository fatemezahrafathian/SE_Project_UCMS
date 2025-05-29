using FluentValidation;
using UCMS.DTOs.TeamDto;
using UCMS.Resources;

namespace UCMS.Services.TeamService;

public class CreateTeamDtoValidator : AbstractValidator<CreateTeamDto>
{
    public CreateTeamDtoValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(Messages.TeamNameIsRequired)
            .MaximumLength(100).WithMessage(Messages.TeamNameMaxLength);
        
        RuleFor(x => x.LeaderStudentNumber)
            .NotEmpty().WithMessage(Messages.LeaderStudentNumberIsRequired)
            .MaximumLength(20).WithMessage(Messages.LeaderStudentNumberMaxLength);
        
        RuleFor(x => x.StudentNumbers)
            .NotNull().WithMessage(Messages.StudentNumbersIsRequired)
            .Must(list => list.Count > 0).WithMessage(Messages.StudentNumbersIsRequired)
            .Must(list =>list.Distinct().Count() == list.Count).WithMessage(Messages.DuplicateStudentNumbers)
            .ForEach(s =>
            {
                s.Must(item => !string.IsNullOrWhiteSpace(item))
                    .WithMessage(Messages.InvalidStudentNumber);
            });
    }
}