using Microsoft.EntityFrameworkCore;
using UCMS.Models;

namespace UCMS.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<Instructor> Instructors { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<ClassStudent> ClassStudents { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<StudentTeam> StudentTeams { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
        modelBuilder.Entity<User>()
            .OwnsOne(u => u.OneTimeCode);

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();
        
        modelBuilder.Entity<Class>()
            .Property(p => p.StartDate)
            .HasColumnType("date");

        modelBuilder.Entity<Class>()
            .Property(p => p.EndDate)
            .HasColumnType("date");
        
        modelBuilder.Entity<ClassStudent>()
            .HasKey(cs => new { cs.ClassId, cs.StudentId });

        modelBuilder.Entity<ClassStudent>()
            .HasOne(cs => cs.Class)
            .WithMany(c => c.ClassStudents)
            .HasForeignKey(cs => cs.ClassId);

        modelBuilder.Entity<ClassStudent>()
            .HasOne(cs => cs.Student)
            .WithMany(s => s.ClassStudents)
            .HasForeignKey(cs => cs.StudentId);

        modelBuilder.Entity<Student>()
            .HasOne(s => s.User)
            .WithOne(u => u.Student)
            .HasForeignKey<Student>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Project>(entity =>
        {
            entity.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.Description)
                .HasMaxLength(500);

            entity.Property(p => p.TotalScore)
                .IsRequired();

            entity.Property(p => p.ProjectType)
                // .HasConversion<string>()  // ذخیره enum به صورت string
                .IsRequired();

            entity.Property(p => p.GroupSize)
                .IsRequired(false);

            entity.Property(p => p.StartDate)
                .IsRequired();

            entity.Property(p => p.EndDate)
                .IsRequired();

            entity.Property(p => p.ProjectFilePath)
                .HasMaxLength(300);

            entity.Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(p => p.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(p => p.Class)
                .WithMany(c => c.Projects)
                .HasForeignKey(p => p.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(p => p.ClassId);
            entity.HasIndex(p => p.ProjectType);
        });

    }
    
}
