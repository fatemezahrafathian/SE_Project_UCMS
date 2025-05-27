using FluentValidation;
using UCMS.DTOs.TeamDto;
using UCMS.Resources;

namespace UCMS.Services.TeamService;

public class UpdateTeamDtoValidator : AbstractValidator<PatchTeamDto>
{

    public UpdateTeamDtoValidator()
    {
        When(x => x.Name != null, () =>
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(Messages.TeamNameIsRequired)
                .MaximumLength(100).WithMessage(Messages.TeamNameMaxLength);
        });

        When(x => x.LeaderStudentNumber != null, () =>
        {
            RuleFor(x => x.LeaderStudentNumber)
                .NotEmpty().WithMessage(Messages.LeaderStudentNumberIsRequired)
                .MaximumLength(20).WithMessage(Messages.LeaderStudentNumberMaxLength);
        });

        When(x => x.AddedStudentNumbers != null, () =>
        {
            RuleForEach(x => x.AddedStudentNumbers)
                .Must(s => !string.IsNullOrWhiteSpace(s))
                .WithMessage(Messages.InvalidStudentNumber);
        });

        When(x => x.DeletedStudentNumbers != null, () =>
        {
            RuleForEach(x => x.DeletedStudentNumbers)
                .Must(s => !string.IsNullOrWhiteSpace(s))
                .WithMessage(Messages.InvalidStudentNumber);
        });

        When(x => x.AddedStudentNumbers != null, () =>
        {
            RuleFor(x => x.AddedStudentNumbers)
                .Must(list => list == null || list.Distinct().Count() == list.Count)
                .WithMessage(Messages.DuplicateStudentNumbers);
        });
        
        When(x => x.DeletedStudentNumbers != null, () =>
        {
            RuleFor(x => x.DeletedStudentNumbers)
                .Must(list => list == null || list.Distinct().Count() == list.Count)
                .WithMessage(Messages.DuplicateStudentNumbers);
        });
        
        When(x => x.AddedStudentNumbers != null && x.DeletedStudentNumbers != null, () =>
        {
            RuleFor(x => x)
            .Must(dto =>
            {
                var overlap = dto.AddedStudentNumbers.Intersect(dto.DeletedStudentNumbers).Any();
                return !overlap;
            })
            .WithMessage(Messages.DuplicateStudentNumbers);
        });

    }

}