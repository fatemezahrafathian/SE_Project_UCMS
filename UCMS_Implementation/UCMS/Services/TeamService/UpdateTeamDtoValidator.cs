using FluentValidation;
using UCMS.DTOs.TeamDto;
using UCMS.Resources;

namespace UCMS.Services.TeamService;

public class UpdateTeamDtoValidator : AbstractValidator<PatchTeamDto>
{
    public UpdateTeamDtoValidator()  // check team size
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

    }

}