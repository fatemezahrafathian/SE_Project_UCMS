using AutoMapper;
using UCMS.DTOs;
using UCMS.DTOs.TeamDto;
using UCMS.Factories;
using UCMS.Models;
using UCMS.Repositories.ProjectRepository.Abstarction;
using UCMS.Repositories.StudentRepository.Abstraction;
using UCMS.Repositories.TeamRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.TeamService.Abstraction;

namespace UCMS.Services.TeamService;

public class TeamService: ITeamService
{
    private readonly ITeamRepository _teamRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IProjectRepository _projectRepository;
    private readonly IMapper _mapper;
    private readonly IStudentRepository _studentRepository;

    public TeamService(ITeamRepository teamRepository, IHttpContextAccessor httpContextAccessor, IProjectRepository projectRepository, IMapper mapper, IStudentRepository studentRepository)
    {
        _teamRepository = teamRepository;
        _httpContextAccessor = httpContextAccessor;
        _projectRepository = projectRepository;
        _mapper = mapper;
        _studentRepository = studentRepository;
    }

    public async Task<ServiceResponse<GetTeamForInstructorDto>> CreateTeam(CreateTeamDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        if(!await _projectRepository.ProjectExists(dto.ProjectId))
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.ProjectNotFound);
        }
        
        if(!await _projectRepository.IsProjectForInstructorAsync(dto.ProjectId, user!.Instructor!.Id))
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.CanNotAccessTheProject);
        }
        
        var validator = new CreateTeamDtoValidator();
        var result = await validator.ValidateAsync(dto);

        if (!result.IsValid)
        {
            var errorMessage = result.Errors.First().ErrorMessage;
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(errorMessage);
        }

        var newTeam = _mapper.Map<Team>(dto);
        
        var students = await _studentRepository.GetStudentsByStudentNumbersAsync(dto.StudentNumbers);
        if (students.Count != dto.StudentNumbers.Count)
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.SomeStudentNumbersNotFound);

        var leader = students.FirstOrDefault(s => s.StudentNumber == dto.LeaderStudentNumber);
        if (leader == null)
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.LeaderStudentIsNotMemeberOfTeam);

        newTeam.StudentTeams = students.Select(s => new StudentTeam
        {
            StudentId = s.Id,
            Student = s,
            Role = s.StudentNumber == dto.LeaderStudentNumber ? TeamRole.Leader : TeamRole.Member,
        }).ToList();

        await _teamRepository.AddTeamAsync(newTeam);

        var responseDto = _mapper.Map<GetTeamForInstructorDto>(newTeam);

        return ServiceResponseFactory.Success(responseDto, Messages.TeamCreatedSuccessfully);
    }

    public async Task<ServiceResponse<GetTeamForInstructorDto>> GetTeamForInstructor(int teamId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var teamEntity = await _teamRepository.GetTeamForInstructorByTeamIdAsync(teamId);
        if (teamEntity == null)
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(
                Messages.TeamNotFound);
        }

        if(!await _teamRepository.IsTeamForInstructor(teamId, user!.Instructor!.Id))
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.CanNotAccessTheTeam);
        }

        var responseDto = _mapper.Map<GetTeamForInstructorDto>(teamEntity);
        return ServiceResponseFactory.Success(responseDto, Messages.TeamFetchedSuccessfully);
    }
    
    public async Task<ServiceResponse<List<GetTeamPreviewForInstructorDto>>> GetProjectTeamsForInstructor(int projectId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        if(!await _projectRepository.ProjectExists(projectId))
        {
            return ServiceResponseFactory.Failure<List<GetTeamPreviewForInstructorDto>>(Messages.ProjectNotFound);
        }
        
        if(!await _projectRepository.IsProjectForInstructorAsync(projectId, user!.Instructor!.Id))
        {
            return ServiceResponseFactory.Failure<List<GetTeamPreviewForInstructorDto>>(Messages.CanNotAccessTheProject);
        }

        var teamEntityList = _teamRepository.GetTeamsForInstructorByProjectIdAsync(projectId);        

        var responseDto = _mapper.Map<List<GetTeamPreviewForInstructorDto>>(teamEntityList);

        return ServiceResponseFactory.Success(responseDto, Messages.TeamsFetchedSuccessfully);
    }
    
    public async Task<ServiceResponse<string>> DeleteTeam(int teamId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var teamEntity = await _teamRepository.GetTeamByTeamIdAsync(teamId);
        if (teamEntity == null)
        {
            return ServiceResponseFactory.Failure<string>(
                Messages.TeamNotFound);
        }

        if(!await _teamRepository.IsTeamForInstructor(teamId, user!.Instructor!.Id))
        {
            return ServiceResponseFactory.Failure<string>(Messages.CanNotAccessTheTeam);
        }

        await _teamRepository.DeleteTeamAsync(teamEntity);
        
        return ServiceResponseFactory.Success<string>(Messages.TeamDeletedSuccessfully);
    }

    public async Task<ServiceResponse<GetTeamForInstructorDto>> UpdateTeamPartial(int teamId, PatchTeamDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var teamEntity = await _teamRepository.GetTeamForInstructorByTeamIdAsync(teamId);
        if (teamEntity == null)
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.TeamNotFound);
        }

        if (!await _teamRepository.IsTeamForInstructor(teamId, user!.Instructor!.Id))
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.CanNotAccessTheTeam);
        }

        var validator = new UpdateTeamDtoValidator();
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errorMessage = result.Errors.First().ErrorMessage;
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(errorMessage);
        }

        var allStudentNumbers = dto.AddedStudentNumbers.Concat(dto.DeletedStudentNumbers)
            .Append(dto.LeaderStudentNumber)
            .Where(sn => !string.IsNullOrWhiteSpace(sn))
            .Distinct()
            .ToList();

        var students = await _studentRepository.GetStudentsByStudentNumbersAsync(allStudentNumbers);
        if (students.Count != allStudentNumbers.Count)
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.SomeStudentNumbersNotFound);
        }

        var studentDict = students.ToDictionary(s => s.StudentNumber);

        var currentStudentNumbers = teamEntity.StudentTeams.Select(st => st.Student.StudentNumber).ToList();

        var alreadyInTeam = dto.AddedStudentNumbers.Intersect(currentStudentNumbers).ToList();
        var notInTeam = dto.DeletedStudentNumbers.Except(currentStudentNumbers).ToList();

        if (alreadyInTeam.Any())
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(
                $"The following students are already in the team and cannot be added again: {string.Join(", ", alreadyInTeam)}");
        }

        if (notInTeam.Any())
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(
                $"The following students are not in the team and cannot be removed: {string.Join(", ", notInTeam)}");
        }

        _mapper.Map(dto, teamEntity);
        
        if (!string.IsNullOrWhiteSpace(dto.LeaderStudentNumber))
        {
            var leaderEntry = teamEntity.StudentTeams.FirstOrDefault(st => st.Student.StudentNumber == dto.LeaderStudentNumber);
            if (leaderEntry == null)
            {
                return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.TeamLeaderIsNotMemeberOfTeam);
            }

            foreach (var st in teamEntity.StudentTeams)
            {
                st.Role = TeamRole.Member;
            }

            leaderEntry.Role = TeamRole.Leader;
        }

        foreach (var sn in dto.AddedStudentNumbers)
        {
            var student = studentDict[sn];
            teamEntity.StudentTeams.Add(new StudentTeam
            {
                StudentId = student.Id,
                Role = TeamRole.Member
            });
        }

        teamEntity.StudentTeams = teamEntity.StudentTeams
            .Where(st => !dto.DeletedStudentNumbers.Contains(st.Student.StudentNumber))
            .ToList();

        teamEntity.UpdatedAt = DateTime.UtcNow;

        await _teamRepository.UpdateTeamAsync(teamEntity);

        var responseDto = _mapper.Map<GetTeamForInstructorDto>(teamEntity);
        return ServiceResponseFactory.Success(responseDto, Messages.TeamUpdatedSuccessfully);
    }
    
}