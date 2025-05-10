using FluentValidation;
using UCMS.DTOs.TeamDto;
using UCMS.Resources;

namespace UCMS.Services.TeamService;

public class CreateTeamDtoValidator : AbstractValidator<CreateTeamDto>
{
    public CreateTeamDtoValidator() // check team size
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(Messages.TeamNameIsRequired)
            .MaximumLength(100).WithMessage(Messages.TeamNameMaxLength);

        RuleFor(x => x.ProjectId)
            .GreaterThanOrEqualTo(0).WithMessage(Messages.ProjectIdIsRequired);

        RuleFor(x => x.LeaderStudentNumber)
            .NotEmpty().WithMessage(Messages.LeaderStudentNumberIsRequired);

        RuleFor(x => x.StudentNumbers)
            .NotNull().WithMessage(Messages.StudentNumbersIsRequired)
            .Must(list => list.Count > 0).WithMessage(Messages.StudentNumbersIsRequired);

        RuleFor(x => x)
            .Must(x => x.StudentNumbers.Contains(x.LeaderStudentNumber))
            .WithMessage(Messages.LeaderStudentIsNotMemeberOfTeam);
    }
}