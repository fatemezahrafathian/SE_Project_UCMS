using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.Models;
using UCMS.Repositories.PhaseRepository;

namespace UCMS_Test.Repositories;

public class PhaseRepositoryTests
{
    private DataContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new DataContext(options);
    }

    [Fact]
    public async Task AddAsync_ShouldAddProjectToDatabase()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repository = new PhaseRepository(context);

        var phase = new Phase
        {
            Id = 1,
            Title = "Test Project",
            Description = "This is a test",
            FileFormats = "pdf, zip"
        };

        // Act
        await repository.AddAsync(phase);

        // Assert
        var savedPhase = await context.Phases.FirstOrDefaultAsync(p => p.Id == 1);
        Assert.NotNull(savedPhase);
        Assert.Equivalent(phase, savedPhase);
    }

    [Fact]
    public async Task GetPhaseByIdAsync_ReturnsPhase_WhenPhaseExists()
    {
        var context = GetInMemoryDbContext();

        var @class = new Class
        {
            Id = 1,
            Title = "Class A",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 }
        };

        var project = new Project
        {
            Id = 1,
            Title = "Project A",
            Class = @class
        };

        var phase = new Phase
        {
            Id = 1,
            Title = "Phase 1",
            Project = project,
            FileFormats = "pdf, zip"
        };

        context.Classes.Add(@class);
        context.Projects.Add(project);
        context.Phases.Add(phase);
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);

        var result = await repo.GetPhaseByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equivalent(phase, result);
    }

    [Fact]
    public async Task GetPhaseByIdAsync_IncludesProject_WhenPhaseExists()
    {
        var context = GetInMemoryDbContext();

        var @class = new Class
        {
            Id = 1,
            Title = "Class A",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 }
        };

        var project = new Project
        {
            Id = 1,
            Title = "Project A",
            Class = @class
        };

        var phase = new Phase
        {
            Id = 1,
            Title = "Phase 1",
            Project = project,
            FileFormats = "pdf, zip"
        };

        context.Classes.Add(@class);
        context.Projects.Add(project);
        context.Phases.Add(phase);
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);

        var result = await repo.GetPhaseByIdAsync(1);

        Assert.NotNull(result);
        Assert.NotNull(result.Project);
        Assert.Equivalent(project, result.Project);
    }

    [Fact]
    public async Task GetPhaseByIdAsync_IncludesClass_WhenPhaseExists()
    {
        var context = GetInMemoryDbContext();

        var @class = new Class
        {
            Id = 1,
            Title = "Class A",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 }
        };

        var project = new Project
        {
            Id = 1,
            Title = "Project A",
            Class = @class
        };

        var phase = new Phase
        {
            Id = 1,
            Title = "Phase 1",
            Project = project,
            FileFormats = "pdf, zip"
        };

        context.Classes.Add(@class);
        context.Projects.Add(project);
        context.Phases.Add(phase);
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);

        var result = await repo.GetPhaseByIdAsync(1);

        Assert.NotNull(result);
        Assert.NotNull(result.Project);
        Assert.Equivalent(@class, result.Project.Class);
    }
    
    [Fact]
    public async Task GetPhaseByIdAsync_ReturnsNull_WhenPhaseNotFound()
    {
        var context = GetInMemoryDbContext();
        var repo = new PhaseRepository(context);

        var result = await repo.GetPhaseByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPhaseWithTeamRelationsByIdAsync_ReturnsPhase_WhenExists()
    {
        var context = GetInMemoryDbContext();
        var phase = new Phase
        {
            Id = 1,
            Title = "Phase 1",
            FileFormats = "pdf, zip"
        };

        context.Phases.Add(phase);
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);

        var result = await repo.GetPhaseWithTeamRelationsByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equivalent(phase, result);
    }

    [Fact]
    public async Task GetPhaseWithTeamRelationsByIdAsync_IncludesStudentTeamPhases()
    {
        var context = GetInMemoryDbContext();

        var phase = new Phase { Id = 2, Title = "Phase 2", FileFormats = "pdf, zip" };
        var stp = new StudentTeamPhase { Id = 1, Phase = phase };
        phase.StudentTeamPhases = new List<StudentTeamPhase> { stp };

        context.Phases.Add(phase);
        context.StudentTeamPhases.Add(stp);
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);

        var result = await repo.GetPhaseWithTeamRelationsByIdAsync(2);

        Assert.Single(result!.StudentTeamPhases);
        Assert.Equivalent(stp, result.StudentTeamPhases.First());
    }

    [Fact]
    public async Task GetPhaseWithTeamRelationsByIdAsync_IncludesStudentTeam()
    {
        var context = GetInMemoryDbContext();

        var team = new Team() { Id = 1, Name = "Team A"};
        var student = new Student() { Id = 1, StudentNumber = "1234"};
        var studentTeam = new StudentTeam { Id = 1, Student = student, Team = team};
        var stp = new StudentTeamPhase { Id = 2, StudentTeam = studentTeam };
        var phase = new Phase { Id = 3, Title = "Phase 3", FileFormats = "pdf, zip", 
            StudentTeamPhases = new List<StudentTeamPhase> { stp } };

        context.Students.Add(student);
        context.Teams.Add(team);
        context.StudentTeams.Add(studentTeam);
        context.StudentTeamPhases.Add(stp);
        context.Phases.Add(phase);
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);

        var result = await repo.GetPhaseWithTeamRelationsByIdAsync(3);

        Assert.NotNull(result!.StudentTeamPhases.First().StudentTeam);
        Assert.Equivalent(studentTeam, result.StudentTeamPhases.First().StudentTeam);
    }

    [Fact]
    public async Task GetPhaseWithTeamRelationsByIdAsync_IncludesStudent()
    {
        var context = GetInMemoryDbContext();

        var team = new Team() { Id = 1, Name = "Team A"};
        var student = new Student() { Id = 1, StudentNumber = "1234"};
        var studentTeam = new StudentTeam { Id = 1, Student = student, Team = team};
        var stp = new StudentTeamPhase { Id = 2, StudentTeam = studentTeam };
        var phase = new Phase { Id = 3, Title = "Phase 3", FileFormats = "pdf, zip", 
            StudentTeamPhases = new List<StudentTeamPhase> { stp } };

        context.Students.Add(student);
        context.Teams.Add(team);
        context.StudentTeams.Add(studentTeam);
        context.StudentTeamPhases.Add(stp);
        context.Phases.Add(phase);
        await context.SaveChangesAsync();
        
        var repo = new PhaseRepository(context);

        var result = await repo.GetPhaseWithTeamRelationsByIdAsync(3);

        Assert.NotNull(result!.StudentTeamPhases.First().StudentTeam.Student);
        Assert.Equivalent(student, result!.StudentTeamPhases.First().StudentTeam.Student);
    }
    
    [Fact]
    public async Task GetPhaseWithTeamRelationsByIdAsync_ReturnsPhaseWithEmptyStudentTeamPhases_WhenNoTeamAssigned()
    {
        var context = GetInMemoryDbContext();

        var phase = new Phase
        {
            Id = 2,
            Title = "Phase without teams",
            FileFormats = "pdf, zip",
            StudentTeamPhases = new List<StudentTeamPhase>()
        };

        context.Phases.Add(phase);
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);

        var result = await repo.GetPhaseWithTeamRelationsByIdAsync(2);

        Assert.NotNull(result);
        Assert.Empty(result!.StudentTeamPhases);
    }

    [Fact]
    public async Task GetPhaseWithTeamRelationsByIdAsync_ReturnsNull_WhenPhaseNotFound()
    {
        var context = GetInMemoryDbContext();
        var repo = new PhaseRepository(context);

        var result = await repo.GetPhaseWithTeamRelationsByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPhaseWithClassStudentRelationsByIdAsync_ReturnsPhase_WhenExists()
    {
        var context = GetInMemoryDbContext();
        var phase = new Phase
        {
            Id = 1,
            Title = "Phase 1",
            FileFormats = "pdf, zip"
        };

        context.Phases.Add(phase);
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);

        var result = await repo.GetPhaseWithTeamRelationsByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equivalent(phase, result);
    }
    
    [Fact]
    public async Task GetPhaseWithClassStudentRelationsByIdAsync_LoadsProject()
    {
        var context = GetInMemoryDbContext();

        var @class = new Class
        {
            Id = 20,
            Title = "Class A",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 }
        };
        var project = new Project { Id = 10, Title = "Project A", Class = @class};
        var phase = new Phase { Id = 2, Title = "Phase 2", Project = project, FileFormats = "pdf, zip" };

        context.Projects.Add(project);
        context.Phases.Add(phase);
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);
        var result = await repo.GetPhaseWithClassStudentRelationsByIdAsync(2);

        Assert.NotNull(result!.Project);
        Assert.Equivalent(project, result.Project);
    }

    [Fact]
    public async Task GetPhaseWithClassStudentRelationsByIdAsync_LoadsClass()
    {
        var context = GetInMemoryDbContext();

        var @class = new Class
        {
            Id = 20,
            Title = "Class A",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 }
        };

        var project = new Project { Id = 11, Title = "Project B", Class = @class };
        var phase = new Phase { Id = 3, Title = "Phase 3", Project = project, FileFormats = "pdf, zip" };

        context.Classes.Add(@class);
        context.Projects.Add(project);
        context.Phases.Add(phase);
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);
        var result = await repo.GetPhaseWithClassStudentRelationsByIdAsync(3);

        Assert.NotNull(result!.Project.Class);
        Assert.Equivalent(@class, result.Project.Class);
    }

    [Fact]
    public async Task GetPhaseWithClassStudentRelationsByIdAsync_LoadsClassStudents()
    {
        var context = GetInMemoryDbContext();

        var student = new Student { Id = 1 };
        var classStudent = new ClassStudent { Student = student };

        var @class = new Class
        {
            Id = 30,
            Title = "Class B",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };

        var project = new Project { Id = 12, Title = "Project A", Class = @class };
        var phase = new Phase { Id = 4, Title = "Phase A", Project = project, FileFormats = "pdf, zip" };

        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Projects.Add(project);
        context.Phases.Add(phase);
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);
        var result = await repo.GetPhaseWithClassStudentRelationsByIdAsync(4);

        Assert.NotNull(result!.Project.Class.ClassStudents);
        Assert.Single(result.Project.Class.ClassStudents);
        Assert.Equivalent(classStudent, result.Project.Class.ClassStudents.First());
    }

    [Fact]
    public async Task GetPhaseWithClassStudentRelationsByIdAsync_LoadsStudent()
    {
        var context = GetInMemoryDbContext();

        var student = new Student { Id = 2 };
        var classStudent = new ClassStudent { Student = student };

        var @class = new Class
        {
            Id = 40,
            Title = "Class B",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };
        
        var project = new Project { Id = 13, Title = "Project 1", Class = @class };
        var phase = new Phase { Id = 5, Title = "Phase 1", Project = project, FileFormats = "pdf, zip" };

        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Projects.Add(project);
        context.Phases.Add(phase);
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);
        var result = await repo.GetPhaseWithClassStudentRelationsByIdAsync(5);

        var loadedStudent = result!.Project.Class.ClassStudents.First().Student;
        Assert.NotNull(loadedStudent);
        Assert.Equivalent(student, loadedStudent);
    }

    [Fact]
    public async Task GetPhaseWithClassStudentRelationsByIdAsync_LoadsStudentUser()
    {
        var context = GetInMemoryDbContext();

        var user = new User
        {
            Id = 10,
            FirstName = "Reza",
            Username = "reza123",
            Email = "reza@example.com",
            PasswordHash = new byte[] { 1, 2, 3 },
            PasswordSalt = new byte[] { 4, 5, 6 }
        };
        
        var student = new Student { Id = 3, User = user };
        var classStudent = new ClassStudent { Student = student };

        var @class = new Class
        {
            Id = 50,
            Title = "Class B",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };

        var project = new Project { Id = 14, Title = "Project 1", Class = @class };
        var phase = new Phase { Id = 6, Title = "Phase 1", Project = project, FileFormats = "pdf, zip" };

        context.Users.Add(user);
        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Projects.Add(project);
        context.Phases.Add(phase);
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);
        var result = await repo.GetPhaseWithClassStudentRelationsByIdAsync(6);

        var loadedUser = result!.Project.Class.ClassStudents.First().Student.User;
        Assert.NotNull(loadedUser);
        Assert.Equivalent(user, loadedUser);
    }

    [Fact]
    public async Task GetPhaseWithClassStudentRelationsByIdAsync_ReturnsPhaseWithNoStudents()
    {
        var context = GetInMemoryDbContext();

        var classEntity = new Class
        {
            Id = 2,
            Title = "Class B",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 },
            ClassStudents = new List<ClassStudent>()
        };
        var project = new Project { Id = 2, Title = "Project B", Class = classEntity };
        var phase = new Phase { Id = 2, Title = "Phase 2", Project = project, FileFormats = "pdf, zip" };

        context.Classes.Add(classEntity);
        context.Projects.Add(project);
        context.Phases.Add(phase);
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);

        var result = await repo.GetPhaseWithClassStudentRelationsByIdAsync(2);

        Assert.Empty(result!.Project.Class.ClassStudents);
    }

    [Fact]
    public async Task GetPhaseWithClassStudentRelationsByIdAsync_ReturnsNull_WhenPhaseNotFound()
    {
        var context = GetInMemoryDbContext();
        var repo = new PhaseRepository(context);

        var result = await repo.GetPhaseWithClassStudentRelationsByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPhasesByProjectIdAsync_ReturnsPhases_ForGivenProject()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var classEntity = new Class
        {
            Id = 2,
            Title = "Class B",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 },
            ClassStudents = new List<ClassStudent>()
        };
        var project = new Project { Id = 1, Title = "Project A", Class = classEntity};
        var phase1 = new Phase { Id = 1, Title = "Phase 1", Project = project, FileFormats = "pdf, zip" };
        var phase2 = new Phase { Id = 2, Title = "Phase 2", Project = project, FileFormats = "pdf, zip" };

        context.Projects.Add(project);
        context.Phases.AddRange(phase1, phase2);
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);

        // Act
        var result = await repo.GetPhasesByProjectIdAsync(1);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Title == "Phase 1");
        Assert.Contains(result, p => p.Title == "Phase 2");
    }

    [Fact]
    public async Task GetPhasesByProjectIdAsync_ReturnsEmptyList_WhenNoPhases()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var classEntity = new Class
        {
            Id = 2,
            Title = "Class B",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 },
            ClassStudents = new List<ClassStudent>()
        };
        var project = new Project { Id = 2, Title = "Empty Project", Class = classEntity};
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);

        // Act
        var result = await repo.GetPhasesByProjectIdAsync(2);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPhasesByProjectIdAsync_ReturnsEmptyList_WhenProjectDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new PhaseRepository(context);

        // Act
        var result = await repo.GetPhasesByProjectIdAsync(999);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingPhase()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var phase = new Phase { Id = 1, Title = "Old Title", FileFormats = "pdf", ProjectId = 10 };
        context.Phases.Add(phase);
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);

        // Act
        phase.Title = "Updated Title";
        await repo.UpdateAsync(phase);

        // Assert
        var updated = await context.Phases.FindAsync(1);
        Assert.NotNull(updated);
        Assert.Equal(phase, updated);
    }

    [Fact]
    public async Task UpdateAsync_DoesNotThrow_WhenPhasesDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new PhaseRepository(context);

        var phase = new Phase { Id = 999, Title = "Non-existing", ProjectId = 20 };

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => repo.UpdateAsync(phase));
        Assert.Null(exception);
    }
    
    [Fact]
    public async Task ExistsWithTitleExceptIdAsync_ReturnsTrue_WhenDuplicateTitleExists()
    {
        var context = GetInMemoryDbContext();

        context.Phases.Add(new Phase { Id = 1, Title = "Phase A", ProjectId = 100, FileFormats = "pdf" });
        context.Phases.Add(new Phase { Id = 2, Title = "Phase A", ProjectId = 100, FileFormats = "pdf" });
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);

        var result = await repo.ExistsWithTitleExceptIdAsync("Phase A", 100, 1);

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsWithTitleExceptIdAsync_ReturnsFalse_WhenOnlyMatchingPhaseIsExcluded()
    {
        var context = GetInMemoryDbContext();

        context.Phases.Add(new Phase { Id = 3, Title = "Phase B", ProjectId = 200, FileFormats = "pdf" });
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);

        var result = await repo.ExistsWithTitleExceptIdAsync("Phase B", 200, 3);

        Assert.False(result);
    }
    
    [Fact]
    public async Task ExistsWithTitleExceptIdAsync_ReturnsFalse_WhenTitleExistsInDifferentProject()
    {
        var context = GetInMemoryDbContext();

        context.Phases.Add(new Phase { Id = 5, Title = "Shared Title", ProjectId = 400, FileFormats = "pdf" });
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);

        var result = await repo.ExistsWithTitleExceptIdAsync("Shared Title", 401, 5);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsWithTitleExceptIdAsync_ReturnsFalse_WhenNoMatchingTitleExists()
    {
        var context = GetInMemoryDbContext();

        context.Phases.Add(new Phase { Id = 4, Title = "Phase C", ProjectId = 300, FileFormats = "pdf" });
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);

        var result = await repo.ExistsWithTitleExceptIdAsync("Nonexistent Title", 300, 4);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesPhase_WhenPhaseExists()
    {
        var context = GetInMemoryDbContext();
        var phase = new Phase { Id = 1, Title = "To be deleted", ProjectId = 100, FileFormats = "pdf" };

        context.Phases.Add(phase);
        await context.SaveChangesAsync();

        var repo = new PhaseRepository(context);
        await repo.DeleteAsync(phase);

        var result = await context.Phases.FindAsync(1);
        Assert.Null(result);
    }
    
    [Fact]
    public async Task DeleteAsync_DoesNothing_WhenPhaseDoesNotExist()
    {
        var context = GetInMemoryDbContext();
        var repo = new PhaseRepository(context);

        var phase = new Phase { Id = 999, Title = "Non-existing", ProjectId = 999, FileFormats = "pdf" };

        var exception = await Record.ExceptionAsync(() => repo.DeleteAsync(phase));

        Assert.Null(exception);
    }
    
    [Fact]
    public async Task GetPhasesCloseDeadLines_ReturnsPhasesWithinBounds()
    {
        var context = GetInMemoryDbContext();
        var token = CancellationToken.None;

        context.Phases.AddRange(
            new Phase { Id = 1, Title = "Inside", FileFormats = "pdf", EndDate = DateTime.UtcNow.AddDays(1), Project = new Project { Title = "Project A", Class = new Class { Title = "Class A", PasswordHash = new byte[] { 1 }, PasswordSalt = new byte[] { 1 } } } },
            new Phase { Id = 2, Title = "Before", FileFormats = "pdf", EndDate = DateTime.UtcNow.AddDays(-5), Project = new Project { Title = "Project A", Class = new Class { Title = "Class A", PasswordHash = new byte[] { 1 }, PasswordSalt = new byte[] { 1 } } } },
            new Phase { Id = 3, Title = "After", FileFormats = "pdf", EndDate = DateTime.UtcNow.AddDays(10), Project = new Project { Title = "Project A", Class = new Class { Title = "Class A", PasswordHash = new byte[] { 1 }, PasswordSalt = new byte[] { 1 } } } }
        );
        await context.SaveChangesAsync(token);

        var repo = new PhaseRepository(context);

        var lower = DateTime.UtcNow;
        var upper = DateTime.UtcNow.AddDays(5);

        var result = await repo.GetPhasesCloseDeadLines(lower, upper, token);

        Assert.Single(result);
        Assert.Equal("Inside", result[0].Title);
    }

    [Fact]
    public async Task GetPhasesCloseDeadLines_ReturnsEmptyList_WhenNoPhasesInRange()
    {
        var context = GetInMemoryDbContext();
        var token = CancellationToken.None;

        context.Phases.Add(new Phase
        {
            Id = 1,
            Title = "Far away",
            FileFormats = "pdf",
            EndDate = DateTime.UtcNow.AddDays(30),
            Project = new Project { Title = "Project 1", Class = new Class { Title = "Class A", PasswordHash = new byte[] { 1 }, PasswordSalt = new byte[] { 1 } } }
        });
        await context.SaveChangesAsync(token);

        var repo = new PhaseRepository(context);

        var lower = DateTime.UtcNow.AddDays(-10);
        var upper = DateTime.UtcNow.AddDays(-5);

        var result = await repo.GetPhasesCloseDeadLines(lower, upper, token);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPhasesCloseDeadLines_LoadsUserDataForStudents()
    {
        var context = GetInMemoryDbContext();
        var token = CancellationToken.None;

        var user = new User { Id = 1, FirstName = "Ali", LastName = "Ahmadi", Email = "ali@example.com", Username = "ali", PasswordHash = new byte[] { 1 }, PasswordSalt = new byte[] { 2 } };
        var student = new Student { Id = 1, User = user };
        var classStudent = new ClassStudent { Student = student };
        var @class = new Class
        {
            Id = 1,
            Title = "Test Class",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };
        var project = new Project { Id = 1, Title = "Project A", Class = @class };
        var phase = new Phase { Id = 1, Title = "Phase A", FileFormats = "pdf", EndDate = DateTime.UtcNow.AddDays(1), Project = project };

        context.Users.Add(user);
        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Projects.Add(project);
        context.Phases.Add(phase);
        await context.SaveChangesAsync(token);

        var repo = new PhaseRepository(context);
        var result = await repo.GetPhasesCloseDeadLines(DateTime.UtcNow, DateTime.UtcNow.AddDays(5), token);

        var loadedUser = result[0].Project.Class.ClassStudents.First().Student.User;

        Assert.NotNull(loadedUser);
        Assert.Equal(1, loadedUser.Id);
    }

    [Fact]
    public async Task GetPhasesCloseStartDate_ReturnsPhasesWithinRange()
    {
        var context = GetInMemoryDbContext();
        var token = CancellationToken.None;

        var now = DateTime.UtcNow;

        var phaseInside = new Phase
        {
            Id = 1,
            Title = "In Range",
            FileFormats = "pdf",
            StartDate = now.AddDays(2),
            Project = new Project
            {
                Title = "Project A",
                Class = new Class
                {
                    Title = "Class A",
                    PasswordHash = new byte[] { 1 },
                    PasswordSalt = new byte[] { 1 }
                }
            }
        };

        var phaseBefore = new Phase
        {
            Id = 2,
            Title = "Before Range",
            FileFormats = "pdf",
            StartDate = now.AddDays(-10),
            Project = new Project
            {
                Title = "Project B",
                Class = new Class
                {
                    Title = "Class A",
                    PasswordHash = new byte[] { 1 },
                    PasswordSalt = new byte[] { 1 }
                }
            }
        };

        var phaseAfter = new Phase
        {
            Id = 3,
            Title = "After Range",
            FileFormats = "pdf",
            StartDate = now.AddDays(10),
            Project = new Project
            {
                Title = "Project C",
                Class = new Class
                {
                    Title = "Class A",
                    PasswordHash = new byte[] { 1 },
                    PasswordSalt = new byte[] { 1 }
                }
            }
        };

        context.Phases.AddRange(phaseInside, phaseBefore, phaseAfter);
        await context.SaveChangesAsync(token);

        var repo = new PhaseRepository(context);
        var result = await repo.GetPhasesCloseStartDate(now.AddDays(1), now.AddDays(5), token);

        Assert.Single(result);
        Assert.Equal("In Range", result[0].Title);
    }

    [Fact]
    public async Task GetPhasesCloseStartDate_ReturnsEmptyList_WhenNoPhaseInRange()
    {
        var context = GetInMemoryDbContext();
        var token = CancellationToken.None;

        var now = DateTime.UtcNow;

        var phase = new Phase
        {
            Id = 1,
            Title = "Too Late",
            FileFormats = "pdf",
            StartDate = now.AddDays(10),
            Project = new Project
            {
                Title = "Project A",
                Class = new Class
                {
                    Title = "Class A",
                    PasswordHash = new byte[] { 1 },
                    PasswordSalt = new byte[] { 1 }
                }
            }
        };

        context.Phases.Add(phase);
        await context.SaveChangesAsync(token);

        var repo = new PhaseRepository(context);
        var result = await repo.GetPhasesCloseStartDate(now.AddDays(-5), now.AddDays(-1), token);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPhasesCloseStartDate_LoadsStudentUser()
    {
        var context = GetInMemoryDbContext();
        var token = CancellationToken.None;

        var user = new User
        {
            Id = 1,
            FirstName = "Reza",
            Email = "reza@example.com",
            Username = "reza",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 2 }
        };

        var student = new Student { Id = 1, User = user };
        var classStudent = new ClassStudent { Student = student };
        var @class = new Class
        {
            Id = 1,
            Title = "Sample Class",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };

        var project = new Project { Id = 1, Title = "Project A", Class = @class };
        var phase = new Phase
        {
            Id = 1,
            Title = "With User",
            FileFormats = "pdf",
            StartDate = DateTime.UtcNow.AddDays(1),
            Project = project
        };

        context.Users.Add(user);
        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Projects.Add(project);
        context.Phases.Add(phase);
        await context.SaveChangesAsync(token);

        var repo = new PhaseRepository(context);
        var result = await repo.GetPhasesCloseStartDate(DateTime.UtcNow, DateTime.UtcNow.AddDays(3), token);

        var loadedUser = result[0].Project.Class.ClassStudents.First().Student.User;
        Assert.NotNull(loadedUser);
        Assert.Equal("Reza", loadedUser.FirstName);
    }

}