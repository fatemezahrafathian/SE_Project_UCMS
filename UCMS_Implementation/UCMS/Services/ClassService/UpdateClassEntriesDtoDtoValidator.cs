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

        // check EntryType to be valid (Phase, Exercise, Exam)
        // check PortionInTotalScore and TotalScore to be valid
        // check sum of PortionInTotalScores to be equal to TotalScore
        // any other check needed
    }
}