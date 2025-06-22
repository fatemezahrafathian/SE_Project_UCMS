using System.Text;
using DocumentFormat.OpenXml.Office2010.Excel;
using UCMS.DTOs;
using UCMS.Repositories.PhaseSubmissionRepository.Abstraction;
using UCMS.Repositories.StudentTeamPhaseRepository.Abstraction;
using UCMS.Repositories.TeamRepository.Abstraction;
using Xunit.Abstractions;

namespace UCMS_Test.Service;
using Moq;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using UCMS.DTOs.PhaseDto;
using UCMS.Models;
using UCMS.Repositories.ClassRepository.Abstraction;
using UCMS.Repositories.PhaseRepository.Abstraction;
using UCMS.Repositories.ProjectRepository.Abstarction;
using UCMS.Services.FileService;
using UCMS.Services.PhaseService;
using UCMS.Resources;

public class PhaseServiceTest
{
    private readonly Mock<IPhaseRepository> _phaseRepoMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextMock = new();
    private readonly Mock<IFileService> _fileServiceMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly PhaseService _service;
    private readonly Mock<IProjectRepository> _projectRepoMock = new();
    private readonly Mock<IStudentClassRepository> _studentClassRepositoryMock = new();
    private readonly Mock<ITeamRepository> _teamRepositoryMock = new();
    private readonly Mock<IPhaseSubmissionRepository> _phaseSubmissionRepositoryMock = new();
    private readonly Mock<IStudentTeamPhaseRepository> _studentTeamPhaseRepositoryMock = new();
    private readonly ITestOutputHelper _output;
    

    public PhaseServiceTest(ITestOutputHelper output)
    {
        _httpContextMock.Setup(x => x.HttpContext!.Items).Returns(new Dictionary<object, object>()!);
        _service = new PhaseService(
            _phaseRepoMock.Object,
            _mapperMock.Object,
            _httpContextMock.Object,
            _projectRepoMock.Object,
            _fileServiceMock.Object,
            _studentClassRepositoryMock.Object,
            _teamRepositoryMock.Object,
            _phaseSubmissionRepositoryMock.Object,
            _studentTeamPhaseRepositoryMock.Object
        );
        _output = output;
    }
    [Fact]
    public async Task Should_Return_Failure_When_Project_Not_Found()
    {
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Project?)null);
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);

        var result = await _service.CreatePhaseAsync(1, new CreatePhaseDto());

        Assert.False(result.Success);
        Assert.Equal(Messages.ProjectNotFound, result.Message);
        _phaseRepoMock.Verify(r => r.AddAsync(It.IsAny<Phase>()), Times.Never);
        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task Should_Return_Failure_When_Instructor_Is_Invalid()
    {
        var project = new Project { Class = new Class { InstructorId = 999 } };
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);

        var result = await _service.CreatePhaseAsync(1, new CreatePhaseDto());

        Assert.False(result.Success);
        Assert.Equal(Messages.InvalidIstructorForThisProject, result.Message);
        _phaseRepoMock.Verify(r => r.AddAsync(It.IsAny<Phase>()), Times.Never);
        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task Should_Return_Failure_When_Phase_Start_Before_Project()
    {
        var dto = new CreatePhaseDto { StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(2) };
        var project = new Project { StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(10), Class = new Class { InstructorId = 1 } };
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);

        var result = await _service.CreatePhaseAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseStartTimeCannotBeBeforeProjectStartTime, result.Message);
        _phaseRepoMock.Verify(r => r.AddAsync(It.IsAny<Phase>()), Times.Never);
        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task Should_Return_Failure_When_Phase_End_After_Project()
    {
        var dto = new CreatePhaseDto
        {
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(11)
        };
        var project = new Project
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(10),
            Class = new Class { InstructorId = 1 }
        };
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);

        var result = await _service.CreatePhaseAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseEndTimeCannotBeAfterProjectEndTime, result.Message);
        _phaseRepoMock.Verify(r => r.AddAsync(It.IsAny<Phase>()), Times.Never);
        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task Should_Return_Failure_When_Phase_Title_Already_Exists()
    {
        var dto = new CreatePhaseDto
        {
            Title = "Phase 1",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 10
        };

        var project = new Project
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(10),
            Class = new Class { InstructorId = 1 },
            Id = 1
        };
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhasesByProjectIdAsync(project.Id))
            .ReturnsAsync(new List<Phase> { new Phase { Title = "Phase 1" } });

        var result = await _service.CreatePhaseAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseAlreadyExists, result.Message);
        _phaseRepoMock.Verify(r => r.AddAsync(It.IsAny<Phase>()), Times.Never);
        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task Should_Return_Failure_When_Dto_Is_Invalid()
    {
        var dto = new CreatePhaseDto
        {
            Title = "",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 10
        };

        var project = new Project
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(10),
            Class = new Class { InstructorId = 1 },
            Id = 1
        };
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhasesByProjectIdAsync(project.Id)).ReturnsAsync(new List<Phase>());

        var result = await _service.CreatePhaseAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.TitleIsRequired, result.Message);
        _phaseRepoMock.Verify(r => r.AddAsync(It.IsAny<Phase>()), Times.Never);
        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);

    }
    [Fact]
    public async Task Should_Save_File_When_PhaseFile_Is_Provided()
    {
        // Arrange
        var user = new User { Instructor = new Instructor { Id = 1 } };
        var project = new Project
        {
            Id = 1,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(10),
            Class = new Class { InstructorId = 1 }
        };

        var fileMock = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("file")), 0, 10, "Data", "test.pdf");
        
        var dto = new CreatePhaseDto
        {
            Title = "Test",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 5,
            PhaseFile = fileMock
        };
        
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _projectRepoMock.Setup(x => x.GetProjectByIdAsync(1)).ReturnsAsync(project);


        _fileServiceMock.Setup(f => f.IsValidExtension(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(f => f.IsValidFileSize(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(f => f.SaveFileAsync(It.IsAny<IFormFile>(), "phases"))
            .ReturnsAsync("path/to/file.pdf");

        _phaseRepoMock.Setup(x => x.GetPhasesByProjectIdAsync(It.IsAny<int>())).ReturnsAsync(new List<Phase>());
        _phaseRepoMock.Setup(x => x.AddAsync(It.IsAny<Phase>())).Returns(Task.CompletedTask);
        
        _mapperMock.Setup(m => m.Map<Phase>(It.IsAny<CreatePhaseDto>())).Returns(new Phase());
        _mapperMock.Setup(m => m.Map<GetPhaseForInstructorDto>(It.IsAny<Phase>())).Returns(new GetPhaseForInstructorDto());

        // Act
        var result = await _service.CreatePhaseAsync(1, dto);

        // Assert
        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), "phases"), Times.Once);
        _phaseRepoMock.Verify(r => r.AddAsync(It.IsAny<Phase>()), Times.Once);
        Assert.True(result.Success);
        Assert.Equal(Messages.PhaseCreatedSuccessfully, result.Message);
    }
    [Fact]
    public async Task Should_Not_Save_File_When_PhaseFile_Is_Null()
    {
        // Arrange
        var dto = new CreatePhaseDto
        {
            Title = "Test",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 5,
            PhaseFile = null
        };
        var user = new User { Instructor = new Instructor { Id = 1 } };
        var project = new Project
        {
            Id = 1,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(10),
            Class = new Class { InstructorId = 1 }
        };
        
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _projectRepoMock.Setup(x => x.GetProjectByIdAsync(1)).ReturnsAsync(project);
        
        _phaseRepoMock.Setup(x => x.GetPhasesByProjectIdAsync(It.IsAny<int>())).ReturnsAsync(new List<Phase>());
        _phaseRepoMock.Setup(x => x.AddAsync(It.IsAny<Phase>())).Returns(Task.CompletedTask);
        
        _mapperMock.Setup(m => m.Map<Phase>(It.IsAny<CreatePhaseDto>())).Returns(new Phase());
        _mapperMock.Setup(m => m.Map<GetPhaseForInstructorDto>(It.IsAny<Phase>())).Returns(new GetPhaseForInstructorDto());

        // Act
        var result = await _service.CreatePhaseAsync(1, dto);

        // Assert
        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), "phases"), Times.Never);
        _phaseRepoMock.Verify(r => r.AddAsync(It.IsAny<Phase>()), Times.Once);
        Assert.True(result.Success);
    }
    [Fact]
    public async Task Should_Return_Validation_Error_And_Not_Save_Or_Add_When_Invalid()
    {
        // Arrange
        var user = new User { Instructor = new Instructor { Id = 1 } };
        var project = new Project
        {
            Id = 1,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(10),
            Class = new Class { InstructorId = 1 }
        };

        var dto = new CreatePhaseDto
        {
            Title = "",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 5,
            PhaseFile = null 
        };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);

        _projectRepoMock.Setup(x => x.GetProjectByIdAsync(1)).ReturnsAsync(project);

        _fileServiceMock.Setup(f => f.IsValidExtension(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(f => f.IsValidFileSize(It.IsAny<IFormFile>())).Returns(true);
        
        _phaseRepoMock.Setup(x => x.GetPhasesByProjectIdAsync(project.Id)).ReturnsAsync(new List<Phase>());
        _phaseRepoMock.Setup(x => x.AddAsync(It.IsAny<Phase>())).Returns(Task.CompletedTask);
        
        _mapperMock.Setup(m => m.Map<Phase>(It.IsAny<CreatePhaseDto>())).Returns(new Phase());
        _mapperMock.Setup(m => m.Map<GetPhaseForInstructorDto>(It.IsAny<Phase>())).Returns(new GetPhaseForInstructorDto());

        // Act
        var result = await _service.CreatePhaseAsync(1, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.TitleIsRequired, result.Message);

        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        _phaseRepoMock.Verify(r => r.AddAsync(It.IsAny<Phase>()), Times.Never);
    }
    [Fact]
    public async Task Should_Create_Phase_When_All_Valid()
    {
        var dto = new CreatePhaseDto
        {
            Title = "Valid Title",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 10,
            PhaseFile = new Mock<IFormFile>().Object
        };

        var project = new Project
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(10),
            Class = new Class { InstructorId = 1 },
            Id = 1
        };

        var user = new User { Instructor = new Instructor { Id = 1 } };

        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhasesByProjectIdAsync(project.Id)).ReturnsAsync(new List<Phase>());
        _fileServiceMock.Setup(f => f.SaveFileAsync(It.IsAny<IFormFile>(), "phases"))
            .ReturnsAsync("saved/file/path.pdf");
        _mapperMock.Setup(m => m.Map<Phase>(It.IsAny<CreatePhaseDto>())).Returns(new Phase());
        _mapperMock.Setup(m => m.Map<GetPhaseForInstructorDto>(It.IsAny<Phase>())).Returns(new GetPhaseForInstructorDto());
        _phaseRepoMock.Setup(r => r.AddAsync(It.IsAny<Phase>())).Returns(Task.CompletedTask);
        _studentTeamPhaseRepositoryMock.Setup(r => r.AddRangeStudentTeamPhaseAsync(It.IsAny<List<StudentTeamPhase>>())).Returns(Task.CompletedTask);
        _teamRepositoryMock.Setup(r => r.GetTeamsWithRelationsByProjectIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Team>
            {
                new Team { StudentTeams = new List<StudentTeam> { new StudentTeam { Id = 1 } } }
            });
        _fileServiceMock.Setup(f => f.IsValidExtension(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(f => f.IsValidFileSize(It.IsAny<IFormFile>())).Returns(true);

        var result = await _service.CreatePhaseAsync(1, dto);
        _output.WriteLine(result.Message);

        Assert.True(result.Success);
        Assert.Equal(Messages.PhaseCreatedSuccessfully, result.Message);

        _phaseRepoMock.Verify(r => r.AddAsync(It.IsAny<Phase>()), Times.Once);
        _studentTeamPhaseRepositoryMock.Verify(r => r.AddRangeStudentTeamPhaseAsync(It.IsAny<List<StudentTeamPhase>>()), Times.Once);
    }

    [Fact]
    public async Task GetPhaseByIdForInstructorAsync_Should_Return_Failure_When_Phase_Not_Found()
    {
        // Arrange
        var phaseId = 10;
        var instructorId = 1;
        var phase = new Phase
        {
            Id = phaseId,
            Project = new Project
            {
                Class = new Class
                {
                    InstructorId = instructorId
                }
            }
        };
        var user = new User
        {
            Instructor = new Instructor { Id = instructorId }
        };
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);

        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(phaseId)).ReturnsAsync((Phase)null);

        // Act
        var result = await _service.GetPhaseByIdForInstructorAsync(phaseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseCantBeAccessed, result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetPhaseByIdForInstructorAsync_Should_Return_Failure_When_User_Not_Authorized()
    {
        // Arrange
        var phaseId = 10;
        var phase = new Phase
        {
            Id = phaseId,
            Project = new Project
            {
                Class = new Class
                {
                    InstructorId = 999 // Not matching
                }
            }
        };

        var user = new User
        {
            Instructor = new Instructor { Id = 1 }
        };
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);

        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(phaseId)).ReturnsAsync(phase);

        // Act
        var result = await _service.GetPhaseByIdForInstructorAsync(phaseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseCantBeAccessed, result.Message);
        Assert.Null(result.Data);
    }
    [Fact]
    public async Task GetPhaseByIdForInstructorAsync_Should_Return_Success_When_Access_Allowed()
    {
        // Arrange
        var instructorId = 1;
        var phaseId = 10;
        var phase = new Phase
        {
            Id = phaseId,
            Project = new Project
            {
                Class = new Class
                {
                    InstructorId = instructorId
                }
            }
        };

        var user = new User
        {
            Instructor = new Instructor { Id = instructorId }
        };

        var expectedDto = new GetPhaseForInstructorDto { phaseId = phaseId };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(phaseId)).ReturnsAsync(phase);
        _mapperMock.Setup(m => m.Map<GetPhaseForInstructorDto>(phase)).Returns(expectedDto);

        // Act
        var result = await _service.GetPhaseByIdForInstructorAsync(phaseId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.PhaseRetrievedSuccessfully, result.Message);
        Assert.Equal(expectedDto, result.Data);
    }
    
    [Fact]
    public async Task Update_Should_Return_Failure_When_Phase_Not_Found()
    {
        var instructorId = 1;
        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(It.IsAny<int>())).ReturnsAsync((Phase?)null);
        
        var user = new User
        {
            Instructor = new Instructor { Id = instructorId }
        };
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);

        var result = await _service.UpdatePhaseAsync(1, new PatchPhaseDto());

        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseNotFound, result.Message);
        _phaseRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Phase>()), Times.Never);
    }
    [Fact]
    public async Task Update_Should_Return_Failure_When_Instructor_Is_Invalid()
    {
        var phase = new Phase
        {
            Project = new Project { Class = new Class { InstructorId = 999 } }
        };

        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(It.IsAny<int>())).ReturnsAsync(phase);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });

        var result = await _service.UpdatePhaseAsync(1, new PatchPhaseDto());

        Assert.False(result.Success);
        Assert.Equal(Messages.InvalidIstructorForThisPhase, result.Message);
    }
    [Fact]
    public async Task Update_Should_Return_Failure_When_StartDate_Before_Project_Start()
    {
        var project = new Project
        {
            StartDate = DateTime.UtcNow,
            Class = new Class
            {
            InstructorId = 1
            }
        };
        var phase = new Phase { Project = project };
        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(It.IsAny<int>())).ReturnsAsync(phase);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = project.Class.InstructorId } });

        var dto = new PatchPhaseDto { StartDate = DateTime.UtcNow.AddDays(-1) };

        var result = await _service.UpdatePhaseAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseStartTimeCannotBeBeforeProjectStartTime, result.Message);
    }
    [Fact]
    public async Task Update_Should_Return_Failure_When_EndDate_After_Project_End()
    {
        var project = new Project
        {
            StartDate = DateTime.UtcNow.AddDays(10),
            Class = new Class
            {
                InstructorId = 1
            }
        };
        var phase = new Phase { Project = project };

        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(It.IsAny<int>())).ReturnsAsync(phase);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = project.Class.InstructorId } });

        var dto = new PatchPhaseDto { EndDate = DateTime.UtcNow.AddDays(11) };

        var result = await _service.UpdatePhaseAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseEndTimeCannotBeAfterProjectEndTime, result.Message);
    }
    [Fact]
    public async Task Update_Should_Return_Failure_When_Title_Already_Exists()
    {
        var project = new Project { Id = 1, Class = new Class { InstructorId = 1 } };
        var phase = new Phase { ProjectId = 1, Project = project };

        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(It.IsAny<int>())).ReturnsAsync(phase);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });

        _phaseRepoMock.Setup(r => r.ExistsWithTitleExceptIdAsync("TestTitle", 1, 1)).ReturnsAsync(true);

        var dto = new PatchPhaseDto { Title = "TestTitle" };

        var result = await _service.UpdatePhaseAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseAlreadyExists, result.Message);
    }
    [Fact]
    public async Task Update_Should_Return_Validation_Error_When_Dto_Is_Invalid()
    {
        var project = new Project { Class = new Class { InstructorId = 1 } };
        var phase = new Phase { Project = project };

        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(It.IsAny<int>())).ReturnsAsync(phase);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });

        var dto = new PatchPhaseDto { PhaseScore = -1 }; 

        var result = await _service.UpdatePhaseAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseScoreMustBePositive, result.Message);
    }
    [Fact]
    public async Task Update_Should_Replace_File_If_New_File_Provided()
    {
        // Arrange
        var user = new User
        {
            Instructor = new Instructor { Id = 1 }
        };

        var context = new DefaultHttpContext();
        context.Items["User"] = user;

        _httpContextMock.Setup(a => a.HttpContext).Returns(context);

        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.FileName).Returns("newfile.pdf");

        var project = new Project
        {
            Id = 10,
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            Class = new Class { InstructorId = 1 }
        };

        var existingPhase = new Phase
        {
            Id = 1,
            ProjectId = 10,
            Project = project,
            PhaseFilePath = "old/file/path.pdf"
        };

        var dto = new PatchPhaseDto
        {
            Title = "Updated",
            PhaseFile = formFileMock.Object,
            FileFormats = "pdf"
        };

        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(1)).ReturnsAsync(existingPhase);
        _phaseRepoMock.Setup(r => r.ExistsWithTitleExceptIdAsync(dto.Title, 10, 1)).ReturnsAsync(false);
        _phaseRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Phase>())).Returns(Task.CompletedTask);
        
        _fileServiceMock.Setup(f => f.DeleteFile("old/file/path.pdf"));
        _fileServiceMock.Setup(f => f.SaveFileAsync(formFileMock.Object, "phases")).ReturnsAsync("new/path/file.pdf");
        _fileServiceMock.Setup(f => f.IsValidExtension(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(f => f.IsValidFileSize(It.IsAny<IFormFile>())).Returns(true);

        

        _mapperMock.Setup(m => m.Map(dto, existingPhase));
        _mapperMock.Setup(m => m.Map<GetPhaseForInstructorDto>(It.IsAny<Phase>()))
                   .Returns(new GetPhaseForInstructorDto { phaseId = 1, Title = "Updated" });

        // Act
        var result = await _service.UpdatePhaseAsync(1, dto);
        
        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.PhaseUpdatedSuccessfully, result.Message);
        _fileServiceMock.Verify(f => f.DeleteFile("old/file/path.pdf"), Times.Once); 
        _fileServiceMock.Verify(f => f.SaveFileAsync(formFileMock.Object, "phases"), Times.Once);
    }
    [Fact]
    public async Task Update_Should_Update_Phase_Successfully_Without_File()
    {
        var project = new Project
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(10),
            Class = new Class { InstructorId = 1 }
        };
        var phase = new Phase
        {
            Project = project,
            Title = "old",
            PhaseFilePath = null
        };

        var dto = new PatchPhaseDto
        {
            Title = "New Title",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(5),
            PhaseFile = null
        };

        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(It.IsAny<int>())).ReturnsAsync(phase);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });
        _phaseRepoMock.Setup(r => r.ExistsWithTitleExceptIdAsync(dto.Title!, project.Id, It.IsAny<int>())).ReturnsAsync(false);
        _fileServiceMock.Setup(f => f.DeleteFile(It.IsAny<string>())).Verifiable();
        _mapperMock.Setup(m => m.Map<PatchPhaseDto, Phase>(It.IsAny<PatchPhaseDto>(), It.IsAny<Phase>()))
            .Callback<PatchPhaseDto, Phase>((src, dest) =>
            {
                if (src.Title != null) dest.Title = src.Title;
                if (src.StartDate.HasValue) dest.StartDate = src.StartDate.Value;
                if (src.EndDate.HasValue) dest.EndDate = src.EndDate.Value;
            });


        var result = await _service.UpdatePhaseAsync(1, dto);

        _phaseRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Phase>()), Times.Once);
        _fileServiceMock.Verify(f => f.DeleteFile(It.IsAny<string>()), Times.Never);
        
        
        Assert.True(result.Success);
        Assert.Equal(Messages.PhaseUpdatedSuccessfully, result.Message);
        Assert.Equal(dto.Title, phase.Title);
    }
    [Fact]
    public async Task Update_Should_Update_Phase_Successfully_With_File()
    {
        // Arrange
        var project = new Project
        {
            Id = 1,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(10),
            Class = new Class { InstructorId = 1 }
        };
        
        var phase = new Phase
        {
            Project = project,
            ProjectId = project.Id,
            PhaseFilePath = "old/path.pdf",
            Title = "Old Title",
            FileFormats = "zip"
        };
        
        var fileContent = "file content";
        var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
        var newFile = new FormFile(fileStream, 0, fileStream.Length, "Data", "file.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };_fileServiceMock
            .Setup(f => f.IsValidExtension(It.IsAny<IFormFile>()))
            .Returns(true);
        _fileServiceMock
            .Setup(f => f.IsValidFileSize(It.IsAny<IFormFile>()))
            .Returns(true);

        var dto = new PatchPhaseDto
        {
            Title = "Updated Title",
            PhaseFile = newFile,
        };
        

        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(It.IsAny<int>())).ReturnsAsync(phase);
        _phaseRepoMock.Setup(r => r.ExistsWithTitleExceptIdAsync(dto.Title!, project.Id, It.IsAny<int>())).ReturnsAsync(false);
        _phaseRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Phase>())).Returns(Task.CompletedTask);
        
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"])
            .Returns(new User { Instructor = new Instructor { Id = 1 } });
        
        _fileServiceMock.Setup(f => f.DeleteFile("old/path.pdf")).Verifiable();
        _fileServiceMock.Setup(f => f.SaveFileAsync(newFile, "phases")).ReturnsAsync("new/path.pdf");
        
        _mapperMock.Setup(m => m.Map<PatchPhaseDto, Phase>(It.IsAny<PatchPhaseDto>(), It.IsAny<Phase>()))
            .Callback<PatchPhaseDto, Phase>((src, dest) =>
            {
                if (src.Title != null) dest.Title = src.Title;
                if (src.StartDate.HasValue) dest.StartDate = src.StartDate.Value;
                if (src.EndDate.HasValue) dest.EndDate = src.EndDate.Value;
            });
        _mapperMock.Setup(m => m.Map<GetPhaseForInstructorDto>(It.IsAny<Phase>()))
            .Returns(new GetPhaseForInstructorDto { Title = dto.Title! });

        // Act
        var result = await _service.UpdatePhaseAsync(1, dto);

        // Assert
        _fileServiceMock.Verify(f => f.DeleteFile("old/path.pdf"), Times.Once);
        _fileServiceMock.Verify(f => f.SaveFileAsync(newFile, "phases"), Times.Once);
        _phaseRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Phase>()), Times.Once);

        Assert.True(result.Success);
        Assert.Equal(Messages.PhaseUpdatedSuccessfully, result.Message);
        Assert.Equal("new/path.pdf", phase.PhaseFilePath);
        Assert.Equal(dto.Title, phase.Title);
    }
    [Fact]
    public async Task Update_Should_Set_UpdatedAt_After_Update()
    {
        var project = new Project
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(10),
            Class = new Class { InstructorId = 1 }
        };
        var phase = new Phase
        {
            Project = project
        };

        var dto = new PatchPhaseDto
        {
            Title = "Some Title"
        };

        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(It.IsAny<int>())).ReturnsAsync(phase);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });
        _phaseRepoMock.Setup(r => r.ExistsWithTitleExceptIdAsync(dto.Title!, project.Id, It.IsAny<int>())).ReturnsAsync(false);

        var beforeUpdate = DateTime.UtcNow;

        var result = await _service.UpdatePhaseAsync(1, dto);

        Assert.True(result.Success);
        Assert.True(phase.UpdatedAt >= beforeUpdate);
    }
    [Fact]
    public async Task Update_Should_Return_Success_When_Data_Is_Valid()
    {
        // Arrange
        var user = new User
        {
            Instructor = new Instructor { Id = 1 }
        };

        var context = new DefaultHttpContext();
        context.Items["User"] = user;

        _httpContextMock.Setup(a => a.HttpContext).Returns(context);

        var project = new Project
        {
            Id = 10,
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            Class = new Class { InstructorId = 1 }
        };

        var existingPhase = new Phase
        {
            Id = 1,
            Title = "Old Phase",
            ProjectId = project.Id,
            Project = project,
            PhaseFilePath = null,
        };

        var dto = new PatchPhaseDto
        {
            Title = "Updated Phase",
            StartDate = new DateTime(2026, 3, 1),
            EndDate = new DateTime(2026, 4, 1),
            PhaseFile = null // No file uploaded
        };

        var expectedMappedResult = new GetPhaseForInstructorDto
        {
            phaseId = 1,
            Title = "Updated Phase"
            // Add other fields if needed
        };

        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(1)).ReturnsAsync(existingPhase);
        _phaseRepoMock.Setup(r => r.ExistsWithTitleExceptIdAsync(dto.Title, project.Id, 1)).ReturnsAsync(false);
        _phaseRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Phase>())).Returns(Task.CompletedTask);

        _mapperMock.Setup(m => m.Map(dto, existingPhase)).Callback(() =>
        {
            existingPhase.Title = dto.Title;
            existingPhase.StartDate = dto.StartDate.Value;
            existingPhase.EndDate = dto.EndDate.Value;
        });

        _mapperMock.Setup(m => m.Map<GetPhaseForInstructorDto>(existingPhase)).Returns(expectedMappedResult);

        var result = await _service.UpdatePhaseAsync(1, dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.PhaseUpdatedSuccessfully, result.Message);
        Assert.Equal("Updated Phase", result.Data.Title);

        _phaseRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Phase>()), Times.Once);
    }
    [Fact]
    public async Task DeletePhaseAsync_Should_Return_Failure_When_Phase_Not_Found()
    {
        // Arrange
        int phaseId = 10;
        var user = new User { Instructor = new Instructor { Id = 1 } };
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);

        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(phaseId)).ReturnsAsync((Phase?)null);

        // Act
        var result = await _service.DeletePhaseAsync(phaseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseNotFound, result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task DeletePhaseAsync_Should_Return_Failure_When_User_Not_Authorized()
    {
        // Arrange
        int phaseId = 10;
        var phase = new Phase
        {
            Id = phaseId,
            PhaseFilePath = "some/path.pdf",
            Project = new Project
            {
                Class = new Class
                {
                    InstructorId = 999 // Doesn't match user instructor id
                }
            }
        };
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(phaseId)).ReturnsAsync(phase);

        // Act
        var result = await _service.DeletePhaseAsync(phaseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseCantBeAccessed, result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task DeletePhaseAsync_Should_Delete_Phase_And_File_When_Access_Allowed_And_File_Exists()
    {
        // Arrange
        int phaseId = 10;
        var phase = new Phase
        {
            Id = phaseId,
            PhaseFilePath = "file/path.pdf",
            Project = new Project
            {
                Class = new Class
                {
                    InstructorId = 1
                }
            }
        };
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(phaseId)).ReturnsAsync(phase);
        _phaseRepoMock.Setup(r => r.DeleteAsync(phase)).Returns(Task.CompletedTask);
        _fileServiceMock.Setup(f => f.DeleteFile(phase.PhaseFilePath!)).Verifiable();

        // Act
        var result = await _service.DeletePhaseAsync(phaseId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ProjectDeletedSuccessfully, result.Message);
        Assert.Equal("Phase deleted successfully", result.Data);

        _phaseRepoMock.Verify(r => r.DeleteAsync(phase), Times.Once);
        _fileServiceMock.Verify(f => f.DeleteFile(phase.PhaseFilePath!), Times.Once);
    }

    [Fact]
    public async Task DeletePhaseAsync_Should_Delete_Phase_Without_Deleting_File_When_FilePath_Null()
    {
        // Arrange
        int phaseId = 10;
        var phase = new Phase
        {
            Id = phaseId,
            PhaseFilePath = null,
            Project = new Project
            {
                Class = new Class
                {
                    InstructorId = 1
                }
            }
        };
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(phaseId)).ReturnsAsync(phase);
        _phaseRepoMock.Setup(r => r.DeleteAsync(phase)).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeletePhaseAsync(phaseId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ProjectDeletedSuccessfully, result.Message);
        Assert.Equal("Phase deleted successfully", result.Data);

        _phaseRepoMock.Verify(r => r.DeleteAsync(phase), Times.Once);
        _fileServiceMock.Verify(f => f.DeleteFile(It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task GetPhasesForInstructor_Should_Return_Failure_When_Project_Not_Found()
    {
        // Arrange
        int projectId = 10;
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync((Project?)null);

        // Act
        var result = await _service.GetPhasesForInstructor(projectId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseCantBeAccessed, result.Message);
        Assert.Null(result.Data);
    }
    [Fact]
    public async Task GetPhasesForInstructor_Should_Return_Failure_When_User_Not_Authorized()
    {
        // Arrange
        int projectId = 10;
        var project = new Project
        {
            Class = new Class
            {
                InstructorId = 999 // Different from user instructor id
            }
        };
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);

        // Act
        var result = await _service.GetPhasesForInstructor(projectId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseCantBeAccessed, result.Message);
        Assert.Null(result.Data);
    }
    [Fact]
    public async Task GetPhasesForInstructor_Should_Return_Success_With_Phases_List_When_Access_Allowed()
    {
        // Arrange
        int projectId = 10;
        var project = new Project
        {
            Class = new Class
            {
                InstructorId = 1
            }
        };
        var user = new User { Instructor = new Instructor { Id = 1 } };

        var phases = new List<Phase>
        {
            new Phase { Id = 1, Title = "Phase 1" },
            new Phase { Id = 2, Title = "Phase 2" }
        };

        var mappedDtos = new List<GetPhasesForInstructorDto>
        {
            new GetPhasesForInstructorDto { phaseId = 1, Title = "Phase 1" },
            new GetPhasesForInstructorDto { phaseId = 2, Title = "Phase 2" }
        };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _phaseRepoMock.Setup(r => r.GetPhasesByProjectIdAsync(projectId)).ReturnsAsync(phases);
        _mapperMock.Setup(m => m.Map<List<GetPhasesForInstructorDto>>(phases)).Returns(mappedDtos);

        // Act
        var result = await _service.GetPhasesForInstructor(projectId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.PhasesRetrievedSuccessfully, result.Message);
        Assert.Equal(mappedDtos, result.Data);
    }
    [Fact]
    public async Task HandleDownloadPhaseFileForInstructorAsync_Should_Return_Failure_When_Phase_Not_Found()
    {
        // Arrange
        int phaseId = 10;
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(phaseId)).ReturnsAsync((Phase?)null);

        // Act
        var result = await _service.HandleDownloadPhaseFileForInstructorAsync(phaseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseCantBeAccessed, result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task HandleDownloadPhaseFileForInstructorAsync_Should_Return_Failure_When_User_Not_Authorized()
    {
        // Arrange
        int phaseId = 10;
        var phase = new Phase
        {
            Project = new Project
            {
                Class = new Class
                {
                    InstructorId = 999 // Different instructor id
                }
            }
        };
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(phaseId)).ReturnsAsync(phase);

        // Act
        var result = await _service.HandleDownloadPhaseFileForInstructorAsync(phaseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseCantBeAccessed, result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task HandleDownloadPhaseFileForInstructorAsync_Should_Return_Failure_When_FilePath_Is_Null_Or_Empty()
    {
        // Arrange
        int phaseId = 10;
        var phase = new Phase
        {
            PhaseFilePath = null,
            Project = new Project
            {
                Class = new Class { InstructorId = 1 }
            }
        };
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(phaseId)).ReturnsAsync(phase);

        // Act
        var result = await _service.HandleDownloadPhaseFileForInstructorAsync(phaseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.FileNotFound, result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task HandleDownloadPhaseFileForInstructorAsync_Should_Return_Failure_When_File_Does_Not_Exist()
    {
        // Arrange
        int phaseId = 10;
        var phase = new Phase
        {
            PhaseFilePath = "some/path.pdf",
            Project = new Project
            {
                Class = new Class { InstructorId = 1 }
            }
        };
        var user = new User { Instructor = new Instructor { Id = 1 } };
    
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(phaseId)).ReturnsAsync(phase);
        _fileServiceMock.Setup(f => f.DownloadFile(phase.PhaseFilePath)).ReturnsAsync((FileDownloadDto?)null);
    
        // Act
        var result = await _service.HandleDownloadPhaseFileForInstructorAsync(phaseId);
    
        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.FileDoesNotExist, result.Message);
        Assert.Null(result.Data);
    }
    
    [Fact]
    public async Task HandleDownloadPhaseFileForInstructorAsync_Should_Return_Success_When_File_Downloaded()
    {
        // Arrange
        int phaseId = 10;
        var phase = new Phase
        {
            PhaseFilePath = "some/path.pdf",
            Project = new Project
            {
                Class = new Class { InstructorId = 1 }
            }
        };
        var user = new User { Instructor = new Instructor { Id = 1 } };

        var fileDto = new FileDownloadDto
        {
            FileBytes = new byte[] { 1, 2, 3 },
            ContentType = null // will be set in service
        };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(phaseId)).ReturnsAsync(phase);
        _fileServiceMock.Setup(f => f.DownloadFile(phase.PhaseFilePath)).ReturnsAsync(fileDto);
        _fileServiceMock.Setup(f => f.GetContentTypeFromPath(phase.PhaseFilePath)).Returns("application/pdf");

        // Act
        var result = await _service.HandleDownloadPhaseFileForInstructorAsync(phaseId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.PhaseFileDownloadedSuccessfully, result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal("application/pdf", result.Data.ContentType);
    }

    [Fact]
    public async Task GetPhaseByIdForStudentAsync_Should_Return_Failure_When_Phase_Not_Found()
    {
        // Arrange
        int phaseId = 10;
        var user = new User { Student = new Student { Id = 1 } };
        
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(phaseId)).ReturnsAsync((Phase?)null);

        // Act
        var result = await _service.GetPhaseByIdForStudentAsync(phaseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseCantBeAccessed, result.Message);
        Assert.Null(result.Data);
    }
    [Fact]
    public async Task GetPhaseByIdForStudentAsync_Should_Return_Failure_When_Student_Not_In_Class()
    {
        // Arrange
        int phaseId = 10;
        var phase = new Phase
        {
            Project = new Project { ClassId = 1 }
        };
        var user = new User { Student = new Student { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(phaseId)).ReturnsAsync(phase);
        _studentClassRepositoryMock.Setup(r => r.IsStudentOfClassAsync(phase.Project.ClassId, user.Student.Id))
                             .ReturnsAsync(false);

        // Act
        var result = await _service.GetPhaseByIdForStudentAsync(phaseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseCantBeAccessed, result.Message);
        Assert.Null(result.Data);
    }
    [Fact]
    public async Task GetPhaseByIdForStudentAsync_Should_Return_Success_When_Student_Has_Access()
    {
        // Arrange
        int phaseId = 10;
        var phase = new Phase
        {
            Project = new Project { ClassId = 1 }
        };
        var user = new User { Student = new Student { Id = 1 } };
        var expectedDto = new GetPhaseForStudentDto();

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(phaseId)).ReturnsAsync(phase);
        _studentClassRepositoryMock.Setup(r => r.IsStudentOfClassAsync(phase.Project.ClassId, user.Student.Id))
                             .ReturnsAsync(true);
        _mapperMock.Setup(m => m.Map<GetPhaseForStudentDto>(phase)).Returns(expectedDto);

        // Act
        var result = await _service.GetPhaseByIdForStudentAsync(phaseId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.PhaseRetrievedSuccessfully, result.Message);
        Assert.Equal(expectedDto, result.Data);
    }
    [Fact]
    public async Task GetPhasesForStudent_Should_Return_Failure_When_Project_Not_Found()
    {
        // Arrange
        int projectId = 5;
        var user = new User { Student = new Student { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync((Project?)null);

        // Act
        var result = await _service.GetPhasesForStudent(projectId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseCantBeAccessed, result.Message);
        Assert.Null(result.Data);
    }
    [Fact]
    public async Task GetPhasesForStudent_Should_Return_Failure_When_Student_Not_In_Class()
    {
        // Arrange
        int projectId = 5;
        var project = new Project { ClassId = 2 };
        var user = new User { Student = new Student { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _studentClassRepositoryMock.Setup(r => r.IsStudentOfClassAsync(project.ClassId, user.Student.Id))
                             .ReturnsAsync(false);

        // Act
        var result = await _service.GetPhasesForStudent(projectId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseCantBeAccessed, result.Message);
        Assert.Null(result.Data);
    }
    [Fact]
    public async Task GetPhasesForStudent_Should_Return_Success_When_Student_Has_Access()
    {
        // Arrange
        int projectId = 5;
        var project = new Project { ClassId = 2 };
        var phases = new List<Phase>
        {
            new Phase { Id = 1 },
            new Phase { Id = 2 }
        };
        var user = new User { Student = new Student { Id = 1 } };
        var expectedDto = new List<GetPhasesForStudentDto>();

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _studentClassRepositoryMock.Setup(r => r.IsStudentOfClassAsync(project.ClassId, user.Student.Id))
                             .ReturnsAsync(true);
        _phaseRepoMock.Setup(r => r.GetPhasesByProjectIdAsync(projectId)).ReturnsAsync(phases);
        _mapperMock.Setup(m => m.Map<List<GetPhasesForStudentDto>>(phases)).Returns(expectedDto);

        // Act
        var result = await _service.GetPhasesForStudent(projectId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.PhasesRetrievedSuccessfully, result.Message);
        Assert.Equal(expectedDto, result.Data);
    }
    [Fact]
    public async Task HandleDownloadPhaseFileForStudentAsync_Should_Return_Failure_When_Phase_Not_Found()
    {
        // Arrange
        int phaseId = 1;
        var user = new User { Student = new Student { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(phaseId)).ReturnsAsync((Phase?)null);

        // Act
        var result = await _service.HandleDownloadPhaseFileForStudentAsync(phaseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseCantBeAccessed, result.Message);
        Assert.Null(result.Data);
    }
    [Fact]
    public async Task HandleDownloadPhaseFileForStudentAsync_Should_Return_Failure_When_Student_Not_In_Class()
    {
        // Arrange
        int phaseId = 1;
        var phase = new Phase
        {
            Project = new Project { ClassId = 10 },
            PhaseFilePath = "somefile.pdf"
        };
        var user = new User { Student = new Student { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(phaseId)).ReturnsAsync(phase);
        _studentClassRepositoryMock.Setup(r => r.IsStudentOfClassAsync(phase.Project.ClassId, user.Student.Id))
                             .ReturnsAsync(false);

        // Act
        var result = await _service.HandleDownloadPhaseFileForStudentAsync(phaseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseCantBeAccessed, result.Message);
        Assert.Null(result.Data);
    }
    [Fact]
    public async Task HandleDownloadPhaseFileForStudentAsync_Should_Return_Failure_When_FilePath_Is_NullOrEmpty()
    {
        // Arrange
        int phaseId = 1;
        var phase = new Phase
        {
            Project = new Project { ClassId = 10 },
            PhaseFilePath = null
        };
        var user = new User { Student = new Student { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(phaseId)).ReturnsAsync(phase);
        _studentClassRepositoryMock.Setup(r => r.IsStudentOfClassAsync(phase.Project.ClassId, user.Student.Id))
                             .ReturnsAsync(true);

        // Act
        var result = await _service.HandleDownloadPhaseFileForStudentAsync(phaseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseOrFileNotFound, result.Message);
        Assert.Null(result.Data);
    }
    [Fact]
    public async Task HandleDownloadPhaseFileForStudentAsync_Should_Return_Failure_When_FileService_Returns_Null()
    {
        // Arrange
        int phaseId = 1;
        var phase = new Phase
        {
            Project = new Project { ClassId = 10 },
            PhaseFilePath = "file.pdf"
        };
        var user = new User { Student = new Student { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(phaseId)).ReturnsAsync(phase);
        _studentClassRepositoryMock.Setup(r => r.IsStudentOfClassAsync(phase.Project.ClassId, user.Student.Id))
                             .ReturnsAsync(true);
        _fileServiceMock.Setup(f => f.DownloadFile(phase.PhaseFilePath)).ReturnsAsync((FileDownloadDto?)null);

        // Act
        var result = await _service.HandleDownloadPhaseFileForStudentAsync(phaseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.FileDoesNotExist, result.Message);
        Assert.Null(result.Data);
    }
    [Fact]
    public async Task HandleDownloadPhaseFileForStudentAsync_Should_Return_Success_When_File_Downloaded()
    {
        // Arrange
        int phaseId = 1;
        var phase = new Phase
        {
            Project = new Project { ClassId = 10 },
            PhaseFilePath = "file.pdf"
        };
        var user = new User { Student = new Student { Id = 1 } };
        var fileDto = new FileDownloadDto();

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhaseByIdAsync(phaseId)).ReturnsAsync(phase);
        _studentClassRepositoryMock
            .Setup(r => r.IsStudentOfClassAsync(phase.Project.ClassId, user.Student.Id))
            .ReturnsAsync(true);
        _fileServiceMock.Setup(f => f.DownloadFile(phase.PhaseFilePath)).ReturnsAsync(fileDto);
        _fileServiceMock.Setup(f => f.GetContentTypeFromPath(phase.PhaseFilePath)).Returns("application/pdf");

        // Act
        var result = await _service.HandleDownloadPhaseFileForStudentAsync(phaseId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.PhaseFileDownloadedSuccessfully, result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(fileDto, result.Data);
        Assert.Equal("application/pdf", result.Data.ContentType); 
    }

}
