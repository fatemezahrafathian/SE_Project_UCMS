using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.Models;
using UCMS.Repositories.ExerciseRepository;

namespace UCMS_Test.Repositories;

public class ExerciseRepositoryTest
{
    private DataContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new DataContext(options);
    }

    [Fact]
    public async Task AddAsync_ShouldAddExerciseToDatabase()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        var exercise = new Exercise
        {
            Id = 1,
            Title = "Exercise A",
            Description = "Test exercise",
            FileFormats = "pdf",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5)
        };

        // Act
        await repo.AddAsync(exercise);

        // Assert
        var saved = await context.Exercises.FirstOrDefaultAsync(e => e.Id == 1);
        Assert.NotNull(saved);
        Assert.Equivalent(exercise, saved);
    }

    [Fact]
    public async Task GetExerciseByIdAsync_ReturnsExercise_WhenExists()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var exercise = new Exercise
        {
            Id = 1,
            Title = "Exercise 1",
            Description = "Test",
            FileFormats = "pdf",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(3),
            Class = new Class
            {
                Id = 10,
                Title = "Class A",
                PasswordHash = new byte[] { 1 },
                PasswordSalt = new byte[] { 1 }
            }
        };
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        var repo = new ExerciseRepository(context);

        // Act
        var result = await repo.GetExerciseByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equivalent(exercise, result);
    }

    [Fact]
    public async Task GetExerciseByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        // Act
        var result = await repo.GetExerciseByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetExerciseByIdAsync_LoadsRelatedClass()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var @class = new Class
        {
            Id = 5,
            Title = "Related Class",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 2 }
        };

        var exercise = new Exercise
        {
            Id = 2,
            Title = "Exercise with Class",
            FileFormats = "pdf",
            Class = @class,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1)
        };

        context.Classes.Add(@class);
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        var repo = new ExerciseRepository(context);

        // Act
        var result = await repo.GetExerciseByIdAsync(2);

        // Assert
        Assert.NotNull(result!.Class);
        Assert.Equivalent(@class, result.Class);
    }

    [Fact]
    public async Task GetExerciseWithClassRelationsByIdAsync_ReturnsExercise_WhenExists()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var user = new User { Id = 1, Username = "user1", Email = "u1@test.com", PasswordHash = new byte[] { 1 }, PasswordSalt = new byte[] { 1 } };
        var student = new Student { Id = 2, User = user };
        var classStudent = new ClassStudent { Student = student };
        var @class = new Class
        {
            Id = 3,
            Title = "Class A",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 2 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };
        var exercise = new Exercise
        {
            Id = 4,
            Title = "Exercise A",
            Description = "Test desc",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(2),
            FileFormats = "pdf, zip",
            Class = @class
        };

        context.Users.Add(user);
        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        var repo = new ExerciseRepository(context);

        // Act
        var result = await repo.GetExerciseWithClassRelationsByIdAsync(4);

        // Assert
        Assert.NotNull(result);
        Assert.Equivalent(exercise, result);
    }

    [Fact]
    public async Task GetExerciseWithClassRelationsByIdAsync_ReturnsNull_WhenExerciseNotFound()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        // Act
        var result = await repo.GetExerciseWithClassRelationsByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetExerciseWithClassRelationsByIdAsync_LoadsRelatedClass()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var user = new User { Id = 1, Username = "user1", Email = "u1@test.com", PasswordHash = new byte[] { 1 }, PasswordSalt = new byte[] { 1 } };
        var student = new Student { Id = 2, User = user };
        var classStudent = new ClassStudent { Student = student };
        var @class = new Class
        {
            Id = 3,
            Title = "Class A",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 2 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };
        var exercise = new Exercise
        {
            Id = 4,
            Title = "Exercise A",
            Description = "Test desc",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(2),
            FileFormats = "pdf, zip",
            Class = @class
        };

        context.Users.Add(user);
        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        var repo = new ExerciseRepository(context);

        // Act
        var result = await repo.GetExerciseWithClassRelationsByIdAsync(4);

        // Assert
        Assert.NotNull(result!.Class);
        Assert.Equivalent(@class, result.Class);
    }

    [Fact]
    public async Task GetExerciseWithClassRelationsByIdAsync_LoadsRelatedClassStudent()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var user = new User { Id = 1, Username = "user1", Email = "u1@test.com", PasswordHash = new byte[] { 1 }, PasswordSalt = new byte[] { 1 } };
        var student = new Student { Id = 2, User = user };
        var classStudent = new ClassStudent { Student = student };
        var @class = new Class
        {
            Id = 3,
            Title = "Class A",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 2 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };
        var exercise = new Exercise
        {
            Id = 4,
            Title = "Exercise A",
            Description = "Test desc",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(2),
            FileFormats = "pdf, zip",
            Class = @class
        };

        context.Users.Add(user);
        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        var repo = new ExerciseRepository(context);

        // Act
        var result = await repo.GetExerciseWithClassRelationsByIdAsync(4);

        // Assert
        Assert.Single(result!.Class.ClassStudents);
        Assert.Equivalent(classStudent, result.Class.ClassStudents.First());
    }

    [Fact]
    public async Task GetExerciseWithClassRelationsByIdAsync_LoadsRelatedStudent()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var user = new User { Id = 1, Username = "user1", Email = "u1@test.com", PasswordHash = new byte[] { 1 }, PasswordSalt = new byte[] { 1 } };
        var student = new Student { Id = 2, User = user };
        var classStudent = new ClassStudent { Student = student };
        var @class = new Class
        {
            Id = 3,
            Title = "Class A",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 2 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };
        var exercise = new Exercise
        {
            Id = 4,
            Title = "Exercise A",
            Description = "Test desc",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(2),
            FileFormats = "pdf, zip",
            Class = @class
        };

        context.Users.Add(user);
        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        var repo = new ExerciseRepository(context);

        // Act
        var result = await repo.GetExerciseWithClassRelationsByIdAsync(4);

        // Assert
        Assert.NotNull(result!.Class.ClassStudents.First().Student);
        Assert.Equivalent(student, result.Class.ClassStudents.First().Student);
    }

    [Fact]
    public async Task GetExerciseWithClassRelationsByIdAsync_LoadsRelatedUser()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var user = new User { Id = 1, Username = "user1", Email = "u1@test.com", PasswordHash = new byte[] { 1 }, PasswordSalt = new byte[] { 1 } };
        var student = new Student { Id = 2, User = user };
        var classStudent = new ClassStudent { Student = student };
        var @class = new Class
        {
            Id = 3,
            Title = "Class A",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 2 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };
        var exercise = new Exercise
        {
            Id = 4,
            Title = "Exercise A",
            Description = "Test desc",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(2),
            FileFormats = "pdf, zip",
            Class = @class
        };

        context.Users.Add(user);
        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        var repo = new ExerciseRepository(context);

        // Act
        var result = await repo.GetExerciseWithClassRelationsByIdAsync(4);

        // Assert
        Assert.NotNull(result!.Class.ClassStudents.First().Student.User);
        Assert.Equivalent(user, result.Class.ClassStudents.First().Student.User);
    }

    [Fact]
    public async Task GetExerciseWithRelationsByIdAsync_ReturnsExercise_WhenExists()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var student = new Student { Id = 1 };
        var classStudent = new ClassStudent { Student = student };
        var @class = new Class
        {
            Id = 2,
            Title = "Class A",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 2 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };
        var exercise = new Exercise
        {
            Id = 3,
            Title = "Exercise X",
            Description = "Sample Exercise",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            FileFormats = "pdf",
            Class = @class
        };

        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        var repo = new ExerciseRepository(context);

        // Act
        var result = await repo.GetExerciseWithRelationsByIdAsync(3);

        // Assert
        Assert.NotNull(result);
        Assert.Equivalent(exercise, result);
    }

    [Fact]
    public async Task GetExerciseWithRelationsByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        // Act
        var result = await repo.GetExerciseWithRelationsByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetExerciseWithRelationsByIdAsync_LoadsClass()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var student = new Student { Id = 1 };
        var classStudent = new ClassStudent { Student = student };
        var @class = new Class
        {
            Id = 2,
            Title = "Class A",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 2 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };
        var exercise = new Exercise
        {
            Id = 3,
            Title = "Exercise X",
            Description = "Sample Exercise",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            FileFormats = "pdf",
            Class = @class
        };

        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        var repo = new ExerciseRepository(context);

        // Act
        var result = await repo.GetExerciseWithRelationsByIdAsync(3);

        // Assert
        Assert.NotNull(result!.Class);
        Assert.Equivalent(@class, result.Class);
    }

    [Fact]
    public async Task GetExerciseWithRelationsByIdAsync_LoadsClassStudents()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var student = new Student { Id = 1 };
        var classStudent = new ClassStudent { Student = student };
        var @class = new Class
        {
            Id = 2,
            Title = "Class A",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 2 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };
        var exercise = new Exercise
        {
            Id = 3,
            Title = "Exercise X",
            Description = "Sample Exercise",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            FileFormats = "pdf",
            Class = @class
        };

        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        var repo = new ExerciseRepository(context);

        // Act
        var result = await repo.GetExerciseWithRelationsByIdAsync(3);

        // Assert
        Assert.Single(result!.Class.ClassStudents);
        Assert.Equivalent(classStudent, result.Class.ClassStudents.First());
    }

    [Fact]
    public async Task GetExerciseWithRelationsByIdAsync_LoadsStudent()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var student = new Student { Id = 1 };
        var classStudent = new ClassStudent { Student = student };
        var @class = new Class
        {
            Id = 2,
            Title = "Class A",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 2 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };
        var exercise = new Exercise
        {
            Id = 3,
            Title = "Exercise X",
            Description = "Sample Exercise",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            FileFormats = "pdf",
            Class = @class
        };

        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        var repo = new ExerciseRepository(context);

        // Act
        var result = await repo.GetExerciseWithRelationsByIdAsync(3);

        // Assert
        Assert.NotNull(result!.Class.ClassStudents.First().Student);
        Assert.Equivalent(student, result.Class.ClassStudents.First().Student);
    }

    [Fact]
    public async Task GetExercisesByClassIdAsync_ReturnsExercisesForClass()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var classId = 10;

        var exercises = new List<Exercise>
        {
            new Exercise { Id = 1, Title = "Ex 1", ClassId = classId, FileFormats = "pdf", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1) },
            new Exercise { Id = 2, Title = "Ex 2", ClassId = classId, FileFormats = "zip", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(2) }
        };

        context.Exercises.AddRange(exercises);
        await context.SaveChangesAsync();

        var repo = new ExerciseRepository(context);

        // Act
        var result = await repo.GetExercisesByClassIdAsync(classId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal(classId, e.ClassId));
    }

    [Fact]
    public async Task GetExercisesByClassIdAsync_ReturnsEmptyList_WhenNoExercisesExistForClass()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        // Act
        var result = await repo.GetExercisesByClassIdAsync(999); 

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetExercisesByClassIdAsync_DoesNotReturnExercisesFromOtherClasses()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var correctClassId = 20;
        var wrongClassId = 30;

        var exercises = new List<Exercise>
        {
            new Exercise { Id = 1, Title = "Correct", ClassId = correctClassId, FileFormats = "pdf", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1) },
            new Exercise { Id = 2, Title = "Wrong", ClassId = wrongClassId, FileFormats = "zip", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(2) }
        };

        context.Exercises.AddRange(exercises);
        await context.SaveChangesAsync();

        var repo = new ExerciseRepository(context);

        // Act
        var result = await repo.GetExercisesByClassIdAsync(correctClassId);

        // Assert
        Assert.Single(result);
        Assert.Equal("Correct", result.First().Title);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingPhase()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var exercise = new Exercise
        {
            Id = 1, Title = "Correct", ClassId = 1, FileFormats = "pdf", StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1)
        };
        
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        var repo = new ExerciseRepository(context);

        // Act
        exercise.Title = "Updated Title";
        await repo.UpdateAsync(exercise);

        // Assert
        var updated = await context.Exercises.FindAsync(1);
        Assert.NotNull(updated);
        Assert.Equivalent(exercise, updated);
    }

    [Fact]
    public async Task UpdateAsync_DoesNotThrow_WhenPhasesDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        var exercise = new Exercise
        {
            Id = 1, Title = "Correct", ClassId = 1, FileFormats = "pdf", StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1)
        };

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => repo.UpdateAsync(exercise));
        Assert.Null(exception);
    }

    [Fact]
    public async Task ExistsWithTitleExceptIdAsync_ReturnsTrue_WhenDuplicateTitleExists()
    {
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        context.Exercises.AddRange(
            new Exercise { Id = 1, Title = "Test", ClassId = 1, FileFormats = "pdf" },
            new Exercise { Id = 2, Title = "Test", ClassId = 1, FileFormats = "pdf" }
        );
        await context.SaveChangesAsync();

        var result = await repo.ExistsWithTitleExceptIdAsync("Test", 1, 1);

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsWithTitleExceptIdAsync_ReturnsFalse_WhenOnlyMatchingExerciseIsExcluded()
    {
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        context.Exercises.Add(new Exercise { Id = 5, Title = "Unique", ClassId = 2, FileFormats = "zip" });
        await context.SaveChangesAsync();

        var result = await repo.ExistsWithTitleExceptIdAsync("Unique", 2, 5);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsWithTitleExceptIdAsync_ReturnsFalse_WhenTitleExistsInDifferentClass()
    {
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        context.Exercises.Add(new Exercise { Id = 10, Title = "SameTitle", ClassId = 1, FileFormats = "docx" });
        await context.SaveChangesAsync();

        var result = await repo.ExistsWithTitleExceptIdAsync("SameTitle", 2, 999); 

        Assert.False(result);
    }
    
    [Fact]
    public async Task ExistsWithTitleExceptIdAsync_ReturnsFalse_WhenNoMatchingTitleExists()
    {
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        context.Exercises.Add(new Exercise { Id = 20, Title = "Alpha", ClassId = 3, FileFormats = "pdf" });
        await context.SaveChangesAsync();

        var result = await repo.ExistsWithTitleExceptIdAsync("Beta", 3, 0);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesExercise_WhenExerciseExists()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        var exercise = new Exercise
        {
            Id = 1,
            Title = "Test Exercise",
            ClassId = 10,
            FileFormats = "pdf"
        };

        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        // Act
        await repo.DeleteAsync(new Exercise { Id = 1 });

        // Assert
        var deleted = await context.Exercises.FindAsync(1);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_DoesNothing_WhenExerciseDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        var fakeExercise = new Exercise
        {
            Id = 999,
            Title = "Non-Existent",
            ClassId = 10,
            FileFormats = "zip"
        };

        // Act
        var exception = await Record.ExceptionAsync(() => repo.DeleteAsync(fakeExercise));

        // Assert
        Assert.Null(exception);
        Assert.Empty(context.Exercises);
    }
    
    [Fact]
    public async Task GetExercisesByStudentIdAsync_ReturnsExercisesForGivenStudent()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        var student = new Student { Id = 1 };
        var classStudent = new ClassStudent { StudentId = 1, Student = student };

        var @class = new Class
        {
            Id = 100,
            Title = "Test Class",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };

        var exercise = new Exercise
        {
            Id = 1,
            Title = "Exercise A",
            Class = @class,
            FileFormats = "pdf"
        };

        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetExercisesByStudentIdAsync(1);

        // Assert
        Assert.Single(result);
        Assert.Equal("Exercise A", result[0].Title);
    }

    [Fact]
    public async Task GetExercisesByStudentIdAsync_ReturnsEmptyList_WhenStudentNotInAnyClass()
    {
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        var @class = new Class
        {
            Id = 200,
            Title = "Empty Class",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 },
            ClassStudents = new List<ClassStudent>() // No students
        };

        var exercise = new Exercise
        {
            Id = 2,
            Title = "Exercise B",
            Class = @class,
            FileFormats = "zip"
        };

        context.Classes.Add(@class);
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        var result = await repo.GetExercisesByStudentIdAsync(studentId: 999); // Not enrolled anywhere

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetExercisesByStudentIdAsync_ReturnsMultipleExercises_WhenStudentInMultipleClasses()
    {
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        var student = new Student { Id = 5 };

        var class1 = new Class
        {
            Id = 1,
            Title = "Class 1",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 },
            ClassStudents = new List<ClassStudent> { new ClassStudent { StudentId = 5 } }
        };

        var class2 = new Class
        {
            Id = 2,
            Title = "Class 2",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 },
            ClassStudents = new List<ClassStudent> { new ClassStudent { StudentId = 5 } }
        };

        var exercises = new List<Exercise>
        {
            new Exercise { Id = 1, Title = "Ex 1", Class = class1, FileFormats = "pdf" },
            new Exercise { Id = 2, Title = "Ex 2", Class = class2, FileFormats = "docx" }
        };

        context.Students.Add(student);
        context.Classes.AddRange(class1, class2);
        context.Exercises.AddRange(exercises);
        await context.SaveChangesAsync();

        var result = await repo.GetExercisesByStudentIdAsync(5);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, e => e.Title == "Ex 1");
        Assert.Contains(result, e => e.Title == "Ex 2");
    }

    [Fact]
    public async Task GetExercisesByInstructorIdAsync_ReturnsExercises_WhenInstructorHasExercises()
    {
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        var @class = new Class
        {
            Id = 1,
            InstructorId = 10,
            Title = "Test Class",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 }
        };

        var exercise = new Exercise
        {
            Id = 1,
            Title = "Exercise A",
            FileFormats = "pdf",
            Class = @class
        };

        context.Classes.Add(@class);
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetExercisesByInstructorIdAsync(10);

        // Assert
        Assert.Single(result);
        Assert.Equal("Exercise A", result[0].Title);
    }

    [Fact]
    public async Task GetExercisesByInstructorIdAsync_ReturnsEmptyList_WhenInstructorHasNoExercises()
    {
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        var @class = new Class
        {
            Id = 2,
            InstructorId = 999,
            Title = "Other Class",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 }
        };

        var exercise = new Exercise
        {
            Id = 2,
            Title = "Exercise B",
            FileFormats = "zip",
            Class = @class
        };

        context.Classes.Add(@class);
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetExercisesByInstructorIdAsync(5); // Instructor 5 has no exercises

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetExercisesByInstructorIdAsync_ReturnsMultipleExercises_WhenInstructorHasMultipleClasses()
    {
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        var class1 = new Class
        {
            Id = 3,
            InstructorId = 20,
            Title = "Class 1",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 }
        };

        var class2 = new Class
        {
            Id = 4,
            InstructorId = 20,
            Title = "Class 2",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 }
        };

        var exercise1 = new Exercise { Id = 3, Title = "Ex 1", FileFormats = "doc", Class = class1 };
        var exercise2 = new Exercise { Id = 4, Title = "Ex 2", FileFormats = "pdf", Class = class2 };

        context.Classes.AddRange(class1, class2);
        context.Exercises.AddRange(exercise1, exercise2);
        await context.SaveChangesAsync();

        var result = await repo.GetExercisesByInstructorIdAsync(20);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, e => e.Title == "Ex 1");
        Assert.Contains(result, e => e.Title == "Ex 2");
    }

    [Fact]
    public async Task GetExercisesCloseDeadLines_ReturnsExercisesWithinDateRange()
    {
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        var exercise = new Exercise
        {
            Id = 1,
            Title = "Deadline Soon",
            FileFormats = "pdf",
            EndDate = DateTime.UtcNow.AddDays(2),
            Class = new Class
            {
                Id = 1,
                Title = "Class A",
                PasswordHash = new byte[] { 1 },
                PasswordSalt = new byte[] { 1 },
                ClassStudents = new List<ClassStudent>()
            }
        };

        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        var lower = DateTime.UtcNow.AddDays(1);
        var upper = DateTime.UtcNow.AddDays(3);

        var result = await repo.GetExercisesCloseDeadLines(lower, upper, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Deadline Soon", result[0].Title);
    }

    [Fact]
    public async Task GetExercisesCloseDeadLines_ReturnsEmptyList_WhenNoExerciseInRange()
    {
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        var exercise = new Exercise
        {
            Id = 2,
            Title = "Out of Range",
            FileFormats = "zip",
            EndDate = DateTime.UtcNow.AddDays(10),
            Class = new Class
            {
                Id = 2,
                Title = "Class A",
                PasswordHash = new byte[] { 1 },
                PasswordSalt = new byte[] { 1 },
                ClassStudents = new List<ClassStudent>()
            }
        };

        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        var lower = DateTime.UtcNow.AddDays(1);
        var upper = DateTime.UtcNow.AddDays(5);

        var result = await repo.GetExercisesCloseDeadLines(lower, upper, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetExercisesCloseDeadLines_IncludesClassStudentUserRelations()
    {
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        var user = new User { Id = 1, Username = "ali", Email = "a@a.com", PasswordHash = new byte[] { 1 }, PasswordSalt = new byte[] { 1 } };
        var student = new Student { Id = 1, User = user };
        var classStudent = new ClassStudent { Student = student };
        var @class = new Class
        {
            Id = 3,
            Title = "Class A",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };

        var exercise = new Exercise
        {
            Id = 3,
            Title = "Exercise With Relations",
            FileFormats = "pdf",
            EndDate = DateTime.UtcNow.AddDays(1),
            Class = @class
        };

        context.Users.Add(user);
        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        var result = await repo.GetExercisesCloseDeadLines(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddDays(2), CancellationToken.None);

        var loadedUser = result.First().Class.ClassStudents.First().Student.User;

        Assert.NotNull(loadedUser);
        Assert.Equal("ali", loadedUser.Username);
    }

    [Fact]
    public async Task GetExercisesCloseStartDate_ReturnsExercisesWithinStartDateRange()
    {
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        var exercise = new Exercise
        {
            Id = 1,
            Title = "Start Soon",
            FileFormats = "pdf",
            StartDate = DateTime.UtcNow.AddDays(2),
            Class = new Class
            {
                Id = 1,
                Title = "Class A",
                PasswordHash = new byte[] { 1 },
                PasswordSalt = new byte[] { 1 },
                ClassStudents = new List<ClassStudent>()
            }
        };

        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        var lower = DateTime.UtcNow.AddDays(1);
        var upper = DateTime.UtcNow.AddDays(3);

        var result = await repo.GetExercisesCloseStartDate(lower, upper, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Start Soon", result[0].Title);
    }

    [Fact]
    public async Task GetExercisesCloseStartDate_IncludesClassStudentUserRelations()
    {
        var context = GetInMemoryDbContext();
        var repo = new ExerciseRepository(context);

        var user = new User { Id = 1, Username = "ali", Email = "a@a.com", PasswordHash = new byte[] { 1 }, PasswordSalt = new byte[] { 1 } };
        var student = new Student { Id = 1, User = user };
        var classStudent = new ClassStudent { Student = student };
        var @class = new Class
        {
            Id = 3,
            Title = "Class A",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };

        var exercise = new Exercise
        {
            Id = 3,
            Title = "Exercise With Relations",
            FileFormats = "pdf",
            StartDate = DateTime.UtcNow.AddDays(1),
            Class = @class
        };

        context.Users.Add(user);
        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        var result = await repo.GetExercisesCloseStartDate(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddDays(2), CancellationToken.None);

        var loadedUser = result.First().Class.ClassStudents.First().Student.User;

        Assert.NotNull(loadedUser);
        Assert.Equal("ali", loadedUser.Username);
    }

}