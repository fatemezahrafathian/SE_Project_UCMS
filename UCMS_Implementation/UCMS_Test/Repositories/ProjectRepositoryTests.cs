using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.Models;
using UCMS.Repositories.ProjectRepository;

namespace UCMS_Test.Repositories;

public class ProjectRepositoryTests
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
        var repository = new ProjectRepository(context);

        var project = new Project
        {
            Id = 1,
            Title = "Test Project",
            Description = "This is a test"
        };

        // Act
        await repository.AddAsync(project);

        // Assert
        var savedProject = await context.Projects.FirstOrDefaultAsync(p => p.Id == 1);
        Assert.NotNull(savedProject);
        Assert.Equivalent(project, savedProject);
    }
    
    [Fact]
    public async Task GetProjectByIdAsync_ReturnsProject_WhenProjectExists()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var project = new Project
        {
            Id = 1,
            Title = "Test Project",
            Description = "Project description",
            Class = new Class
            {
                Id = 100,
                Title = "Class 1",
                PasswordHash = new byte[] { 0x00 },  
                PasswordSalt = new byte[] { 0x01 }   
            }
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var repo = new ProjectRepository(context);

        // Act
        var result = await repo.GetProjectByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equivalent(project, result);
    }
    
    [Fact]
    public async Task GetProjectByIdAsync_ReturnsNull_WhenProjectDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new ProjectRepository(context);

        // Act
        var result = await repo.GetProjectByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetProjectByIdAsync_IncludesClass_WhenProjectIsLoaded()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var project = new Project
        {
            Id = 1,
            Title = "Test Project",
            Description = "Project description",
            Class = new Class
            {
                Id = 100,
                Title = "Class 1",
                PasswordHash = new byte[] { 0x00 },  
                PasswordSalt = new byte[] { 0x01 }   
            }
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var repo = new ProjectRepository(context);

        // Act
        var result = await repo.GetProjectByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result!.Class);
        Assert.Equivalent(project.Class, result.Class);
    }
    
    [Fact]
    public async Task GetSimpleProjectByIdAsync_ReturnsProject_WhenProjectExists()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var project = new Project
        {
            Id = 1,
            Title = "Simple Project",
            Description = "Test only project entity"
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var repo = new ProjectRepository(context);

        // Act
        var result = await repo.GetSimpleProjectByIdAsync(1);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equivalent(project, result);
    }

    [Fact]
    public async Task GetSimpleProjectByIdAsync_ReturnsNull_WhenProjectDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new ProjectRepository(context);

        // Act
        var result = await repo.GetSimpleProjectByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSimpleProjectByIdAsync_DoesNotIncludeClass()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using (var context = new DataContext(options))
        {
            var classEntity = new Class
            {
                Id = 1,
                Title = "Test Class",
                PasswordHash = new byte[] { 1 },
                PasswordSalt = new byte[] { 2 }
            };

            var project = new Project
            {
                Id = 2,
                Title = "Project With Class",
                ClassId = 1,
                Class = classEntity
            };

            context.Classes.Add(classEntity);
            context.Projects.Add(project);
            await context.SaveChangesAsync();
        }

        // Act & Assert
        await using (var context = new DataContext(options))
        {
            var repo = new ProjectRepository(context);
            var result = await repo.GetSimpleProjectByIdAsync(2);

            Assert.NotNull(result);

            Assert.Null(result.Class);
        }
    }

    [Fact]
    public async Task GetProjectWithRelationsByIdAsync_ReturnsProject_WhenProjectExists()
    {
        var context = GetInMemoryDbContext();

        var student = new Student { Id = 1, StudentNumber = "1234" };

        var cls = new Class
        {
            Id = 1,
            Title = "Class 1",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 2 },
            ClassStudents = new List<ClassStudent>
            {
                new ClassStudent { ClassId = 1, StudentId = 1, Student = student }
            }
        };

        var project = new Project
        {
            Id = 1,
            Title = "Test Project",
            Class = cls
        };

        context.Students.Add(student);
        context.Classes.Add(cls);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var repo = new ProjectRepository(context);

        // Act
        var result = await repo.GetProjectWithRelationsByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equivalent(project, result);
    }

    [Fact]
    public async Task GetProjectWithRelationsByIdAsync_IncludesClass_WhenClassExists()
    {
        var context = GetInMemoryDbContext();

        var student = new Student { Id = 1, StudentNumber = "1234" };

        var cls = new Class
        {
            Id = 1,
            Title = "Class 1",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 2 },
            ClassStudents = new List<ClassStudent>
            {
                new ClassStudent { ClassId = 1, StudentId = 1, Student = student }
            }
        };

        var project = new Project
        {
            Id = 1,
            Title = "Test Project",
            Class = cls
        };

        context.Students.Add(student);
        context.Classes.Add(cls);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var repo = new ProjectRepository(context);

        // Act
        var result = await repo.GetProjectWithRelationsByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Class);
        Assert.Equivalent(cls, result.Class);
        // Assert.NotEmpty(result.Class.ClassStudents);
        // Assert.Equal(1, result.Class.ClassStudents.First().StudentId);
    }

    [Fact]
    public async Task GetProjectWithRelationsByIdAsync_IncludesClassStudents_WhenClassStudentsExists()
    {
        var context = GetInMemoryDbContext();

        var student = new Student { Id = 1, StudentNumber = "1234" };

        var cls = new Class
        {
            Id = 1,
            Title = "Class 1",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 2 },
            ClassStudents = new List<ClassStudent>
            {
                new ClassStudent { ClassId = 1, StudentId = 1, Student = student }
            }
        };

        var project = new Project
        {
            Id = 1,
            Title = "Test Project",
            Class = cls
        };

        context.Students.Add(student);
        context.Classes.Add(cls);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var repo = new ProjectRepository(context);

        // Act
        var result = await repo.GetProjectWithRelationsByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Class);
        Assert.NotEmpty(result.Class.ClassStudents);
        Assert.Equal(1, result.Class.ClassStudents.First().StudentId);
    }

    [Fact]
    public async Task GetProjectWithRelationsByIdAsync_ReturnsProjectWithEmptyStudentList_WhenRelationsDoesNotExist()
    {
        var context = GetInMemoryDbContext();

        var @class = new Class
        {
            Id = 2,
            Title = "Empty Class",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 },
            ClassStudents = new List<ClassStudent>()
        };

        var project = new Project
        {
            Id = 2,
            Title = "Project No Students",
            Class = @class
        };

        context.Classes.Add(@class);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var repo = new ProjectRepository(context);

        // Act
        var result = await repo.GetProjectWithRelationsByIdAsync(2);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Class);
        Assert.Empty(result.Class.ClassStudents);
    }
    
    [Fact]
    public async Task GetProjectWithRelationsByIdAsync_ReturnsNull_WhenProjectDoesNotExist()
    {
        var context = GetInMemoryDbContext();
        var repo = new ProjectRepository(context);

        var result = await repo.GetProjectWithRelationsByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingProject()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var project = new Project
        {
            Id = 1,
            Title = "Original Title",
            Description = "Original Description"
        };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var repo = new ProjectRepository(context);

        // Act
        project.Title = "Updated Title";
        project.Description = "Updated Description";
        await repo.UpdateAsync(project);

        // Assert
        var updated = await context.Projects.FindAsync(1);
        Assert.NotNull(updated);
        Assert.Equal("Updated Title", updated.Title);
        Assert.Equal("Updated Description", updated.Description);
    }

    [Fact]
    public async Task UpdateAsync_DoesNotThrow_WhenProjectDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new ProjectRepository(context);

        var newProject = new Project
        {
            Id = 2,
            Title = "Inserted by Update",
            Description = "EF InMemory behavior"
        };

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => repo.UpdateAsync(newProject));
        Assert.Null(exception);
    }

    [Fact]
    public async Task DeleteAsync_RemovesProjectSuccessfully()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var project = new Project
        {
            Id = 1,
            Title = "Project to delete"
        };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var repository = new ProjectRepository(context);

        // Act
        await repository.DeleteAsync(project);

        // Assert
        var deletedProject = await context.Projects.FindAsync(1);
        Assert.Null(deletedProject);
    }

    [Fact]
    public async Task DeleteAsync_DoesNotThrow_WhenProjectDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repository = new ProjectRepository(context);

        var project = new Project
        {
            Id = 999,
            Title = "Non-existent project"
        };

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => repository.DeleteAsync(project));
        Assert.Null(exception);
    }
    
    [Fact]
    public async Task FilterProjectsForInstructorAsync_ReturnsOnlyInstructorProjects()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var instructor1 = new Instructor { Id = 1, EmployeeCode = "1234" };
        var instructor2 = new Instructor { Id = 2, EmployeeCode = "2345" };

        var class1 = new Class
        {
            Id = 1,
            Title = "Class A",
            InstructorId = 1,
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 }
        };
        var class2 = new Class
        {
            Id = 2,
            Title = "Class B",
            InstructorId = 2,
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 }
        };

        var project1 = new Project { Id = 1, Title = "Project 1", Class = class1, EndDate = DateTime.UtcNow.AddDays(1) };
        var project2 = new Project { Id = 2, Title = "Project 2", Class = class2, EndDate = DateTime.UtcNow.AddDays(1) };

        context.Projects.AddRange(project1, project2);
        context.Classes.AddRange(class1, class2);
        context.Instructors.AddRange(instructor1, instructor2);
        await context.SaveChangesAsync();

        var repo = new ProjectRepository(context);

        // Act
        var result = await repo.FilterProjectsForInstructorAsync(1, null, null, null, "enddate", false);

        // Assert
        Assert.Single(result);
        Assert.Equivalent(project1, result[0]);
    }
    
    [Fact]
    public async Task FilterProjectsForInstructorAsync_FiltersByProjectTitle()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var instructorId = 1;

        var cls = new Class { Id = 1, 
            Title = "C1", 
            InstructorId = instructorId, 
            PasswordHash = new byte[] { 1 }, 
            PasswordSalt = new byte[] { 1 } 
        };

        var p1 = new Project { Id = 1, Title = "AI Project", Class = cls, EndDate = DateTime.UtcNow };
        var p2 = new Project { Id = 2, Title = "Web Project", Class = cls, EndDate = DateTime.UtcNow };

        context.Classes.Add(cls);
        context.Projects.AddRange(p1, p2);
        await context.SaveChangesAsync();

        var repo = new ProjectRepository(context);

        // Act
        var result = await repo.FilterProjectsForInstructorAsync(instructorId, "AI", null, null, "enddate", false);

        // Assert
        Assert.Single(result);
        Assert.Equivalent(p1, result[0]);
    }

    [Fact]
    public async Task FilterProjectsForInstructorAsync_FiltersByProjectByClassTitle()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var instructorId = 1;

        var cls1 = new Class { Id = 1, 
            Title = "AI", 
            InstructorId = instructorId, 
            PasswordHash = new byte[] { 1 }, 
            PasswordSalt = new byte[] { 1 } 
        };

        var cls2 = new Class { Id = 2, 
            Title = "Web", 
            InstructorId = instructorId, 
            PasswordHash = new byte[] { 1 }, 
            PasswordSalt = new byte[] { 1 } 
        };

        var p1 = new Project { Id = 1, Title = "AI Project", Class = cls1, EndDate = DateTime.UtcNow };
        var p2 = new Project { Id = 2, Title = "Web Project", Class = cls2, EndDate = DateTime.UtcNow };

        context.Classes.AddRange(cls1, cls2);
        context.Projects.AddRange(p1, p2);
        await context.SaveChangesAsync();

        var repo = new ProjectRepository(context);

        // Act
        var result = await repo.FilterProjectsForInstructorAsync(instructorId, null, "AI", null, "enddate", false);

        // Assert
        Assert.Single(result);
        Assert.Equivalent(p1, result[0]);
    }

    [Fact]
    public async Task FilterProjectsForInstructorAsync_FiltersByProjectByNotStartedStatus()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var instructorId = 1;

        var cls = new Class { Id = 1, 
            Title = "C1", 
            InstructorId = instructorId, 
            PasswordHash = new byte[] { 1 }, 
            PasswordSalt = new byte[] { 1 } 
        };

        var p1 = new Project { Id = 1, Title = "AI Project", Class = cls, StartDate = DateTime.UtcNow.AddDays(1) };
        var p2 = new Project { Id = 2, Title = "Web Project", Class = cls, EndDate = DateTime.UtcNow };

        context.Classes.Add(cls);
        context.Projects.AddRange(p1, p2);
        await context.SaveChangesAsync();

        var repo = new ProjectRepository(context);

        // Act
        var result = await repo.FilterProjectsForInstructorAsync(instructorId, null, null, 0, "enddate", false);

        // Assert
        Assert.Single(result);
        Assert.Equivalent(p1, result[0]);
    }

    [Fact]
    public async Task FilterProjectsForInstructorAsync_FiltersByProjectByInProgressStatus()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var instructorId = 1;

        var cls = new Class { Id = 1, 
            Title = "C1", 
            InstructorId = instructorId, 
            PasswordHash = new byte[] { 1 }, 
            PasswordSalt = new byte[] { 1 } 
        };

        var p1 = new Project { Id = 1, Title = "AI Project", Class = cls, EndDate = DateTime.UtcNow.AddDays(1) };
        var p2 = new Project { Id = 2, Title = "Web Project", Class = cls, EndDate = DateTime.UtcNow };

        context.Classes.Add(cls);
        context.Projects.AddRange(p1, p2);
        await context.SaveChangesAsync();

        var repo = new ProjectRepository(context);

        // Act
        var result = await repo.FilterProjectsForInstructorAsync(instructorId, null, null, 1, "enddate", false);

        // Assert
        Assert.Single(result);
        Assert.Equivalent(p1, result[0]);
    }
    
    [Fact]
    public async Task FilterProjectsForInstructorAsync_FiltersByProjectByCompletedStatus()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var instructorId = 1;

        var cls = new Class { Id = 1, 
            Title = "C1", 
            InstructorId = instructorId, 
            PasswordHash = new byte[] { 1 }, 
            PasswordSalt = new byte[] { 1 } 
        };

        var p1 = new Project { Id = 1, Title = "AI Project", Class = cls, EndDate = DateTime.UtcNow };
        var p2 = new Project { Id = 2, Title = "Web Project", Class = cls, EndDate = DateTime.UtcNow.AddDays(1) };

        context.Classes.Add(cls);
        context.Projects.AddRange(p1, p2);
        await context.SaveChangesAsync();

        var repo = new ProjectRepository(context);

        // Act
        var result = await repo.FilterProjectsForInstructorAsync(instructorId, null, null, 2, "enddate", false);

        // Assert
        Assert.Single(result);
        Assert.Equivalent(p1, result[0]);
    }

    private async Task<ProjectRepository> SetupRepoWithSampleProjectsForOrdering()
    {
        var context = GetInMemoryDbContext();
        var class1 = new Class { Id = 1, Title = "Class A", InstructorId = 1, PasswordHash = new byte[] { 1 }, PasswordSalt = new byte[] { 1 } };
        var class2 = new Class { Id = 2, Title = "Class B", InstructorId = 1, PasswordHash = new byte[] { 1 }, PasswordSalt = new byte[] { 1 } };

        var project1 = new Project { Id = 1, Title = "Alpha", EndDate = new DateTime(2025, 1, 1), Class = class1 };
        var project2 = new Project { Id = 2, Title = "Beta", EndDate = new DateTime(2025, 6, 1), Class = class2 };

        context.Classes.AddRange(class1, class2);
        context.Projects.AddRange(project1, project2);
        await context.SaveChangesAsync();

        return new ProjectRepository(context);
    }

    [Fact]
    public async Task FilterProjects_OrderByEndDate_Ascending()
    {
        var repo = await SetupRepoWithSampleProjectsForOrdering();

        var result = await repo.FilterProjectsForInstructorAsync(1, null, null, null, "enddate", false);

        Assert.Equal(2, result.Count);
        Assert.True(result[0].EndDate < result[1].EndDate);
    }

    [Fact]
    public async Task FilterProjects_OrderByEndDate_Descending()
    {
        var repo = await SetupRepoWithSampleProjectsForOrdering();

        var result = await repo.FilterProjectsForInstructorAsync(1, null, null, null, "enddate", true);

        Assert.Equal(2, result.Count);
        Assert.True(result[0].EndDate > result[1].EndDate);
    }

    [Fact]
    public async Task FilterProjects_OrderByTitle_Ascending()
    {
        var repo = await SetupRepoWithSampleProjectsForOrdering();

        var result = await repo.FilterProjectsForInstructorAsync(1, null, null, null, "title", false);

        Assert.Equal(2, result.Count);
        Assert.True(string.CompareOrdinal(result[0].Title, result[1].Title) < 0);
    }

    [Fact]
    public async Task FilterProjects_OrderByTitle_Descending()
    {
        var repo = await SetupRepoWithSampleProjectsForOrdering();

        var result = await repo.FilterProjectsForInstructorAsync(1, null, null, null, "title", true);

        Assert.Equal(2, result.Count);
        Assert.True(string.CompareOrdinal(result[0].Title, result[1].Title) > 0);
    }

    [Fact]
    public async Task FilterProjects_OrderByClassTitle_Ascending()
    {
        var repo = await SetupRepoWithSampleProjectsForOrdering();

        var result = await repo.FilterProjectsForInstructorAsync(1, null, null, null, "classtitle", false);

        Assert.Equal(2, result.Count);
        Assert.True(string.CompareOrdinal(result[0].Title, result[1].Title) < 0);
    }
    
    [Fact]
    public async Task FilterProjects_OrderByClassTitle_Descending()
    {
        var repo = await SetupRepoWithSampleProjectsForOrdering();

        var result = await repo.FilterProjectsForInstructorAsync(1, null, null, null, "classtitle", true);

        Assert.Equal(2, result.Count);
        Assert.True(string.CompareOrdinal(result[0].Title, result[1].Title) > 0);
    }

    [Fact]
    public async Task IsProjectNameDuplicateAsync_ReturnsTrue_WhenDuplicateExistsInSameClass()
    {
        var context = GetInMemoryDbContext();

        context.Projects.Add(new Project
        {
            Id = 1,
            Title = "Project A",
            ClassId = 10
        });

        await context.SaveChangesAsync();

        var repo = new ProjectRepository(context);

        var result = await repo.IsProjectNameDuplicateAsync(10, "Project A");

        Assert.True(result);
    }
    
    [Fact]
    public async Task IsProjectNameDuplicateAsync_ReturnsFalse_WhenProjectInDifferentClass()
    {
        var context = GetInMemoryDbContext();

        context.Projects.Add(new Project
        {
            Id = 2,
            Title = "Project A",
            ClassId = 99
        });

        await context.SaveChangesAsync();

        var repo = new ProjectRepository(context);

        var result = await repo.IsProjectNameDuplicateAsync(10, "Project A");

        Assert.False(result);
    }

    [Fact]
    public async Task IsProjectNameDuplicateAsync_ReturnsFalse_WhenNoSuchProjectExists()
    {
        var context = GetInMemoryDbContext();

        context.Projects.Add(new Project
        {
            Id = 3,
            Title = "Another Project",
            ClassId = 10
        });

        await context.SaveChangesAsync();

        var repo = new ProjectRepository(context);

        var result = await repo.IsProjectNameDuplicateAsync(10, "Project A");

        Assert.False(result);
    }
    
    [Fact]
    public async Task GetProjectsByClassIdAsync_ReturnsProjectsSortedByEndDateDescending()
    {
        var context = GetInMemoryDbContext();

        var projects = new List<Project>
        {
            new Project { Id = 1, Title = "P1", EndDate = new DateTime(2024, 6, 1), ClassId = 100 },
            new Project { Id = 2, Title = "P2", EndDate = new DateTime(2024, 7, 1), ClassId = 100 },
            new Project { Id = 3, Title = "P3", EndDate = new DateTime(2024, 5, 1), ClassId = 100 }
        };

        context.Projects.AddRange(projects);
        await context.SaveChangesAsync();

        var repo = new ProjectRepository(context);

        var result = await repo.GetProjectsByClassIdAsync(100);

        Assert.Equal(3, result.Count);
        Assert.Equal(new[] { 2, 1, 3 }, result.Select(p => p.Id));
    }

    [Fact]
    public async Task GetProjectsByClassIdAsync_ReturnsEmptyList_WhenNoProjectsExist()
    {
        var context = GetInMemoryDbContext();

        // کلاس با پروژه ندارد
        context.Projects.Add(new Project
        {
            Id = 10,
            Title = "Irrelevant",
            EndDate = DateTime.UtcNow,
            ClassId = 999
        });

        await context.SaveChangesAsync();

        var repo = new ProjectRepository(context);

        var result = await repo.GetProjectsByClassIdAsync(500);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetProjectsByClassIdAsync_OnlyReturnsProjectsForSpecifiedClass()
    {
        var context = GetInMemoryDbContext();

        context.Projects.AddRange(
            new Project { Id = 1, Title = "C1-P1", EndDate = DateTime.UtcNow, ClassId = 1 },
            new Project { Id = 2, Title = "C2-P1", EndDate = DateTime.UtcNow, ClassId = 2 }
        );

        await context.SaveChangesAsync();

        var repo = new ProjectRepository(context);

        var result = await repo.GetProjectsByClassIdAsync(1);

        Assert.Single(result);
        Assert.Equal(1, result.First().ClassId);
        Assert.Equal("C1-P1", result.First().Title);
    }

}