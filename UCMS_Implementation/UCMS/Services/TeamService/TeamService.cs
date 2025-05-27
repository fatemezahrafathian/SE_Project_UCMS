using AutoMapper;
using ClosedXML.Excel;
using UCMS.DTOs;
using UCMS.DTOs.TeamDto;
using UCMS.Factories;
using UCMS.Models;
using UCMS.Repositories.ClassRepository.Abstraction;
using UCMS.Repositories.ProjectRepository.Abstarction;
using UCMS.Repositories.StudentRepository.Abstraction;
using UCMS.Repositories.TeamRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.TeamService.Abstraction;

namespace UCMS.Services.TeamService;

public class TeamService : ITeamService
{
    private readonly ITeamRepository _teamRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IProjectRepository _projectRepository;
    private readonly IMapper _mapper;
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentClassRepository _studentClassRepository;

    public TeamService(ITeamRepository teamRepository, IHttpContextAccessor httpContextAccessor,
        IProjectRepository projectRepository, IMapper mapper, IStudentRepository studentRepository,
        IStudentClassRepository studentClassRepository)
    {
        _teamRepository = teamRepository;
        _httpContextAccessor = httpContextAccessor;
        _projectRepository = projectRepository;
        _mapper = mapper;
        _studentRepository = studentRepository;
        _studentClassRepository = studentClassRepository;
    }

    public async Task<ServiceResponse<GetTeamForInstructorDto>> CreateTeam(int projectId, CreateTeamDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var project = await _projectRepository.GetSimpleProjectByIdAsync(projectId);
        if (project is null)
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.ProjectNotFound);
        }

        if (!await _projectRepository.IsProjectForInstructorAsync(projectId, user!.Instructor!.Id))
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

        if (dto.StudentNumbers.Count > project!.GroupSize)
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.TeamSizeOutOfLimit);
        }

        // returns string null
        var allStudentNumbersOfClass = await _studentClassRepository.GetStudentNumbersOfClass(project.ClassId);
        var allStudentNumbersOfTeams = await _teamRepository.GetStudentNumbersOfProjectTeams(projectId); // seprate studentTeam repository

        var notInClass = dto.StudentNumbers.Except(allStudentNumbersOfClass).ToList();
        var inAnotherTeam = dto.StudentNumbers.Intersect(allStudentNumbersOfTeams).ToList();

        if (notInClass.Any())
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.SomeStudentsNotInClass);
        }

        if (inAnotherTeam.Any())
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.SomeStudentsAlreadyInAnotherTeam);
        }

        var isLeaderAMember = dto.StudentNumbers.Contains(dto.LeaderStudentNumber);
        if (!isLeaderAMember)
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.LeaderStudentNotInTeam);
        }
        
        var students = await _studentRepository.GetStudentsByStudentNumbersAsync(dto.StudentNumbers);

        var newTeam = _mapper.Map<Team>(dto);

        newTeam.ProjectId = projectId;
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

        if (!await _teamRepository.IsTeamInInstructorClasses(teamId, user!.Instructor!.Id))
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.CanNotAccessTheTeam);
        }

        var responseDto = _mapper.Map<GetTeamForInstructorDto>(teamEntity);
        return ServiceResponseFactory.Success(responseDto, Messages.TeamFetchedSuccessfully);
    }

    public async Task<ServiceResponse<GetTeamForStudentDto>> GetTeamForStudent(int teamId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var teamEntity = await _teamRepository.GetTeamForStudentByTeamIdAsync(teamId);
        if (teamEntity == null)
        {
            return ServiceResponseFactory.Failure<GetTeamForStudentDto>(
                Messages.TeamNotFound);
        }

        if (!await _teamRepository.IsTeamInStudentClasses(teamId, user!.Student!.Id))
        {
            return ServiceResponseFactory.Failure<GetTeamForStudentDto>(Messages.CanNotAccessTheProject);
        }
        
        var responseDto = _mapper.Map<GetTeamForStudentDto>(teamEntity);
        return ServiceResponseFactory.Success(responseDto, Messages.TeamFetchedSuccessfully);
    }

    public async Task<ServiceResponse<List<GetTeamPreviewDto>>> GetProjectTeamsForInstructor(int projectId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        if (!await _projectRepository.ProjectExists(projectId))
        {
            return ServiceResponseFactory.Failure<List<GetTeamPreviewDto>>(Messages.ProjectNotFound);
        }

        if (!await _projectRepository.IsProjectForInstructorAsync(projectId, user!.Instructor!.Id))
        {
            return ServiceResponseFactory
                .Failure<List<GetTeamPreviewDto>>(Messages.CanNotAccessTheProject);
        }

        var teamEntityList = await _teamRepository.GetTeamsByProjectIdAsync(projectId);

        var responseDto = _mapper.Map<List<GetTeamPreviewDto>>(teamEntityList);

        return ServiceResponseFactory.Success(responseDto, Messages.TeamsFetchedSuccessfully);
    }

    public async Task<ServiceResponse<List<GetTeamPreviewDto>>> GetProjectTeamsForStudent(int projectId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        if (!await _projectRepository.ProjectExists(projectId))
        {
            return ServiceResponseFactory.Failure<List<GetTeamPreviewDto>>(Messages.ProjectNotFound);
        }

        if (!await _projectRepository.IsProjectForStudentAsync(projectId, user!.Student!.Id))
        {
            return ServiceResponseFactory.Failure<List<GetTeamPreviewDto>>(Messages.CanNotAccessTheProject);
        }

        var teamEntityList = await _teamRepository.GetTeamsByProjectIdAsync(projectId);

        var responseDto = _mapper.Map<List<GetTeamPreviewDto>>(teamEntityList);

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

        if (!await _teamRepository.IsTeamInInstructorClasses(teamId, user!.Instructor!.Id))
        {
            return ServiceResponseFactory.Failure<string>(Messages.CanNotAccessTheTeam);
        }

        await _teamRepository.DeleteTeamAsync(teamEntity);

        return ServiceResponseFactory.Success<string>(Messages.TeamDeletedSuccessfully);
    }

    public async Task<ServiceResponse<GetTeamForInstructorDto>> UpdateTeamPartial(int teamId, PatchTeamDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var teamEntity = await _teamRepository.GetTeamWithStudentTeamsByIdAsync(teamId); // joined enough ot need more
        if (teamEntity == null)
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.TeamNotFound);
        }

        if (!await _teamRepository.IsTeamInInstructorClasses(teamId, user!.Instructor!.Id))
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

        var currentStudentNumbers = teamEntity.StudentTeams.Select(st => st.Student.StudentNumber).ToList();
        var addedStudentNumbers = dto.AddedStudentNumbers ?? new List<string>();
        var deletedStudentNumbers = dto.DeletedStudentNumbers ?? new List<string>();

        var project = await _projectRepository.GetSimpleProjectByIdAsync(teamEntity.ProjectId);
        var newTeamSize = currentStudentNumbers.Count + addedStudentNumbers.Count - deletedStudentNumbers.Count;
        if (newTeamSize > project!.GroupSize)
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.TeamSizeOutOfLimit);
        }

        var alreadyInTeam = addedStudentNumbers.Intersect(currentStudentNumbers).ToList();
        var notInTeam = deletedStudentNumbers.Except(currentStudentNumbers).ToList();

        if (alreadyInTeam.Any())
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.SomeAddingStudentAlreadyInTeam);
        }

        if (notInTeam.Any())
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.SomeDeleteingStudentNotInTeam);
        }


        var allStudentNumbersOfClass = await _studentClassRepository.GetStudentNumbersOfClass(project.ClassId);
        var allStudentNumbersOfTeams = await _teamRepository.GetStudentNumbersOfProjectTeams(project.Id); // move to studentTeam repository

        var notInClass = addedStudentNumbers.Except(allStudentNumbersOfClass).ToList();
        var inAnotherTeam = addedStudentNumbers.Intersect(allStudentNumbersOfTeams).ToList();

        if (notInClass.Any())
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.SomeStudentsNotInClass);
        }

        if (inAnotherTeam.Any())
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.SomeStudentsAlreadyInAnotherTeam);
        }


        if (!string.IsNullOrWhiteSpace(dto.LeaderStudentNumber))
        {
            var isInCurrentTeam = currentStudentNumbers.Contains(dto.LeaderStudentNumber);
            var isInAddedList = addedStudentNumbers.Contains(dto.LeaderStudentNumber);

            if (!isInCurrentTeam && !isInAddedList)
            {
                return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages.LeaderStudentNotInTeam);
            }
        }

        var currentLeaderStudentNumber = teamEntity.StudentTeams
            .FirstOrDefault(st => st.Role == TeamRole.Leader)?.Student.StudentNumber;

        if (!string.IsNullOrWhiteSpace(currentLeaderStudentNumber) &&
            deletedStudentNumbers.Contains(currentLeaderStudentNumber) &&
            string.IsNullOrWhiteSpace(dto.LeaderStudentNumber))
        {
            return ServiceResponseFactory.Failure<GetTeamForInstructorDto>(Messages
                .MustSpecifyNewLeaderWhenDeletingCurrentLeader);
        }

        var addedStudentIds = await _studentRepository.GetStudentsByStudentNumbersAsync(addedStudentNumbers);
        var deletedStudentIds = await _studentRepository.GetStudentsByStudentNumbersAsync(deletedStudentNumbers);

        _mapper.Map(dto, teamEntity);

        foreach (var studentTeam in addedStudentIds.Select(s => new StudentTeam
                 {
                     StudentId = s.Id,
                     Role = TeamRole.Member
                 }))
        {
            teamEntity.StudentTeams.Add(studentTeam);
        }

        var toRemove = teamEntity.StudentTeams
            .Where(st => deletedStudentIds.Any(d => d.Id == st.StudentId))
            .ToList();

        foreach (var studentTeam in toRemove)
        {
            teamEntity.StudentTeams.Remove(studentTeam);
        }

        if (!string.IsNullOrWhiteSpace(dto.LeaderStudentNumber))
        {
            var currentLeader = teamEntity.StudentTeams.FirstOrDefault(st => st.Role == TeamRole.Leader);
            currentLeader!.Role = TeamRole.Member;

            var newLeader =
                teamEntity.StudentTeams.FirstOrDefault(st => st.Student.StudentNumber == dto.LeaderStudentNumber);
            newLeader!.Role = TeamRole.Leader;
        }

        teamEntity.UpdatedAt = DateTime.UtcNow;

        await _teamRepository.UpdateTeamAsync(teamEntity);

        var responseDto = _mapper.Map<GetTeamForInstructorDto>(teamEntity);
        return ServiceResponseFactory.Success(responseDto, Messages.TeamUpdatedSuccessfully);
    }

    public async Task<ServiceResponse<GetTeamTemplateFileDto>> GetTeamTemplateFile(int projectId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var project = await _projectRepository.GetSimpleProjectByIdAsync(projectId);
        if (project is null)
        {
            return ServiceResponseFactory.Failure<GetTeamTemplateFileDto>(Messages.ProjectNotFound);
        }

        if (!await _projectRepository.IsProjectForInstructorAsync(projectId, user!.Instructor!.Id))
        {
            return ServiceResponseFactory.Failure<GetTeamTemplateFileDto>(Messages.CanNotAccessTheProject);
        }
        
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Teams Template");

        worksheet.Cell(1, 1).Value = "TeamName";
        worksheet.Cell(1, 2).Value = "LeaderStudentNumber";
        for (int i = 1; i <= project.GroupSize-1; i++)
        {
            worksheet.Cell(1, i + 2).Value = $"StudentNumber{i}";
        }

        await using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Seek(0, SeekOrigin.Begin);

        var fileBytes = stream.ToArray();

        var result = new GetTeamTemplateFileDto
        {
            FileName = "TeamTemplate.xlsx",
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",  // use appsetting
            FileContent = fileBytes
        };

        return ServiceResponseFactory.Success(result, Messages.TeamTemplateFileGeneratedSuccessfully);
    }
    
    public async Task<ServiceResponse<List<GetTeamFileValidationResultDto>>> ValidateTeamFileAsync(int projectId, IFormFile file)
    {
        // check file null or empty
        // check file template
        // create with file dto validator to check all errors

        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var project = await _projectRepository.GetSimpleProjectByIdAsync(projectId);
        if (project is null)
        {
            return ServiceResponseFactory.Failure<List<GetTeamFileValidationResultDto>>(Messages.ProjectNotFound);
        }

        if (!await _projectRepository.IsProjectForInstructorAsync(projectId, user!.Instructor!.Id))
        {
            return ServiceResponseFactory.Failure<List<GetTeamFileValidationResultDto>>(Messages.CanNotAccessTheProject);
        }
        
        using var workbook = new XLWorkbook(file.OpenReadStream());
        var worksheet = workbook.Worksheet(1);

        var validator = new CreateTeamDtoValidator();

        var validationResults = new List<GetTeamFileValidationResultDto>();

        int row = 2;

        while (!string.IsNullOrWhiteSpace(worksheet.Cell(row, 1).GetString()))
        {
            var teamName = worksheet.Cell(row, 1).GetString().Trim();
            var leaderNumber = worksheet.Cell(row, 2).GetString().Trim();
            var studentNumbers = new List<string>();

            for (int i = 2; i < 2 + project.GroupSize; i++)
            {
                var studentNumber = worksheet.Cell(row, i).GetString().Trim();
                if (!string.IsNullOrWhiteSpace(studentNumber))
                    studentNumbers.Add(studentNumber);
            }

            var dto = new CreateTeamDto
            {
                Name = teamName,
                LeaderStudentNumber = leaderNumber,
                StudentNumbers = studentNumbers
            };

            // اعتبارسنجی اولیه با FluentValidation
            var errors = new List<string>();

            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                errors.AddRange(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            // چک تیم سایز
            if (studentNumbers.Count > project.GroupSize)
            {
                errors.Add(Messages.TeamSizeOutOfLimit);
            }

            // چک دانشجوهایی که در کلاس نیستند
            var allStudentNumbersOfClass = await _studentClassRepository.GetStudentNumbersOfClass(project.ClassId);
            var allStudentNumbersOfTeams = await _teamRepository.GetStudentNumbersOfProjectTeams(projectId); // move to studentTeamRepository

            var notInClass = studentNumbers.Except(allStudentNumbersOfClass).ToList();
            if (notInClass.Any())
            {
                errors.Add(Messages.SomeStudentsNotInClass);
            }

            // چک دانشجوهایی که در تیم دیگری هستند
            var inAnotherTeam = studentNumbers.Intersect(allStudentNumbersOfTeams).ToList();
            if (inAnotherTeam.Any())
            {
                errors.Add(Messages.SomeStudentsAlreadyInAnotherTeam);
            }

            // چک اینکه لیدر جزو اعضاست
            if (!studentNumbers.Contains(leaderNumber))
            {
                errors.Add(Messages.LeaderStudentNotInTeam);
            }

            validationResults.Add(new GetTeamFileValidationResultDto
            {
                RowNumber = row,
                Team = dto,
                IsValid = !errors.Any(),
                Errors = errors
            });

            row++;
        }

        return ServiceResponseFactory.Success(validationResults, Messages.TeamFileValidatedSuccessfuly);
    }

    public async Task<ServiceResponse<string>> CreateTeamsFromFileAsync(int projectId, List<CreateTeamDto> validTeams)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var project = await _projectRepository.GetSimpleProjectByIdAsync(projectId);
        if (project is null)
        {
            return ServiceResponseFactory.Failure<string>(Messages.ProjectNotFound);
        }

        if (!await _projectRepository.IsProjectForInstructorAsync(projectId, user!.Instructor!.Id))
        {
            return ServiceResponseFactory.Failure<string>(Messages.CanNotAccessTheProject);
        }

        // جمع کردن تمام شماره دانشجویی‌ها
        var allStudentNumbers = validTeams.SelectMany(t => t.StudentNumbers).Distinct().ToList();
        
        // یکجا گرفتن دانشجوها از دیتابیس
        var students = await _studentRepository.GetStudentsByStudentNumbersAsync(allStudentNumbers);
        var studentDict = students.ToDictionary(s => s.StudentNumber, s => s);

        var teams = new List<Team>();

        foreach (var teamDto in validTeams)
        {
            var team = _mapper.Map<Team>(teamDto);
            team.ProjectId = projectId;

            foreach (var studentNumber in teamDto.StudentNumbers)
            {
                var studentTeam = new StudentTeam
                {
                    StudentId = studentDict[studentNumber].Id,
                    Role = studentNumber == teamDto.LeaderStudentNumber ? TeamRole.Leader : TeamRole.Member
                };

                team.StudentTeams.Add(studentTeam);
            }

            teams.Add(team);
        }

        // دسته‌ای ذخیره کن
        await _teamRepository.AddTeamsAsync(teams);

        return ServiceResponseFactory.Success("null", Messages.TeamsCreatedSuccessfully);
    }

    
}