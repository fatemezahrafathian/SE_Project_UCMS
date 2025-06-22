using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.DTOs;
using UCMS.Models;
using UCMS.Repositories.ProjectRepository.Abstarction;

namespace UCMS.Repositories.ProjectRepository;

public class ProjectRepository: IProjectRepository
{
    private readonly DataContext _context;

    public ProjectRepository(DataContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Project project)
    {
        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();
    }
    public async Task<Project?> GetProjectByIdAsync(int projectId)
    {
        return await _context.Projects
            .Include(p => p.Class)
            .FirstOrDefaultAsync(p => p.Id == projectId);
        // return await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
    }

    public async Task<Project?> GetSimpleProjectByIdAsync(int projectId)
    {
        return await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId);
    }

    public async Task UpdateAsync(Project project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteAsync(Project project)
    {
        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
    }
    public async Task<List<Project>> FilterProjectsForInstructorAsync(int instructorId, string? title, string? classTitle, int? projectStatus, string orderBy, bool descending)
    {
        var query = _context.Projects
            .Include(p => p.Class)
            .Where(p => p.Class.InstructorId == instructorId)
            .AsQueryable();

        query = ApplyCommonFilters(query, title, classTitle, projectStatus, orderBy, descending);
        return await query.ToListAsync();
    }

    public async Task<List<Project>> FilterProjectsForStudentAsync(int studentId, string? title, string? classTitle, int? projectStatus,string orderBy, bool descending)
    {
        var query = _context.Projects
            .Include(p => p.Class)
            .Where(p => p.Class.ClassStudents.Any(cs => cs.StudentId == studentId))
            .AsQueryable();

        query = ApplyCommonFilters(query, title, classTitle, projectStatus, orderBy, descending);
        return await query.ToListAsync();
    }


    private IQueryable<Project> ApplyCommonFilters(IQueryable<Project> query, string? title, string? classTitle, int? projectStatus, string orderBy,bool descending)
    {
        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(p => p.Title.Contains(title));

        if (!string.IsNullOrWhiteSpace(classTitle))
            query = query.Where(p => p.Class.Title.Contains(classTitle));

        if (projectStatus.HasValue)
        {
            var now = DateTime.UtcNow;

            switch ((ProjectStatus)projectStatus.Value)
            {
                case ProjectStatus.NotStarted:
                    query = query.Where(p => now < p.StartDate);
                    break;
                case ProjectStatus.InProgress:
                    query = query.Where(p => now >= p.StartDate && now <= p.EndDate);
                    break;
                case ProjectStatus.Completed:
                    query = query.Where(p => now > p.EndDate);
                    break;
            }
        }

        // Ordering
        query = (orderBy.ToLower()) switch
        {
            "enddate" => descending ? query.OrderByDescending(p => p.EndDate) : query.OrderBy(p => p.EndDate),
            "title" => descending ? query.OrderByDescending(p => p.Title) : query.OrderBy(p => p.Title),
            "classtitle" => descending ? query.OrderByDescending(p => p.Class.Title) : query.OrderBy(p => p.Class.Title),
            _ => query.OrderBy(p => p.EndDate)
        };

        return query;
    }

    // public async Task<List<Project>> FilterProjectsForInstructorSortedByEndDateAscendingAsync(int instructorId, string? title, string? classTitle, int? projectStatus)
    // {
    //     var query = _context.Projects
    //         .Include(p => p.Class)
    //         .Where(c => c.Class.InstructorId == instructorId)
    //         .AsQueryable();
    //
    //     if (!string.IsNullOrWhiteSpace(title))
    //     {
    //         query = query.Where(c => c.Title.Contains(title));
    //     }
    //     if (!string.IsNullOrWhiteSpace(classTitle))
    //     {
    //         query = query.Where(c => c.Class.Title.Contains(classTitle));
    //     }
    //
    //     if (projectStatus.HasValue)
    //     {
    //         var now = DateTime.UtcNow;
    //
    //         switch ((ProjectStatus)projectStatus.Value)
    //         {
    //             case ProjectStatus.NotStarted:
    //                 query = query.Where(p => now < p.StartDate);
    //                 break;
    //             case ProjectStatus.InProgress:
    //                 query = query.Where(p => now >= p.StartDate && now <= p.EndDate);
    //                 break;
    //             case ProjectStatus.Completed:
    //                 query = query.Where(p => now > p.EndDate);
    //                 break;
    //         }
    //     }
    //     
    //     var items = await query
    //         .OrderBy(c => c.EndDate) 
    //         .ToListAsync();
    //
    //     return items;
    // }
    // public async Task<List<Project>> FilterProjectsForStudentSortedByEndDateAscendingAsync(int studentId, string? title, string? classTitle, int? projectStatus)
    // {
    //     var query = _context.Projects
    //         .Include(p => p.Class)
    //         .Where(p => p.Class.ClassStudents.Any(cs => cs.StudentId == studentId))
    //         .AsQueryable();
    //
    //     if (!string.IsNullOrWhiteSpace(title))
    //     {
    //         query = query.Where(p => p.Title.Contains(title));
    //     }
    //
    //     if (!string.IsNullOrWhiteSpace(classTitle))
    //     {
    //         query = query.Where(p => p.Class.Title.Contains(classTitle));
    //     }
    //
    //     if (projectStatus.HasValue)
    //     {
    //         var now = DateTime.UtcNow;
    //
    //         switch ((ProjectStatus)projectStatus.Value)
    //         {
    //             case ProjectStatus.NotStarted:
    //                 query = query.Where(p => now < p.StartDate);
    //                 break;
    //             case ProjectStatus.InProgress:
    //                 query = query.Where(p => now >= p.StartDate && now <= p.EndDate);
    //                 break;
    //             case ProjectStatus.Completed:
    //                 query = query.Where(p => now > p.EndDate);
    //                 break;
    //         }
    //     }
    //
    //     var result = await query
    //         .OrderBy(p => p.EndDate)
    //         .ToListAsync();
    //
    //     return result;
    // }
    // public async Task<List<Project>> FilterProjectsForInstructorSortedByEndDateDescendingAsync(int instructorId, string? title, string? classTitle, int? projectStatus)
    // {
    //     var query = _context.Projects
    //         .Include(p => p.Class)
    //         .Where(c => c.Class.InstructorId == instructorId)
    //         .AsQueryable();
    //
    //     if (!string.IsNullOrWhiteSpace(title))
    //     {
    //         query = query.Where(c => c.Title.Contains(title));
    //     }
    //     if (!string.IsNullOrWhiteSpace(classTitle))
    //     {
    //         query = query.Where(c => c.Class.Title.Contains(classTitle));
    //     }
    //
    //     if (projectStatus.HasValue)
    //     {
    //         var now = DateTime.UtcNow;
    //
    //         switch ((ProjectStatus)projectStatus.Value)
    //         {
    //             case ProjectStatus.NotStarted:
    //                 query = query.Where(p => now < p.StartDate);
    //                 break;
    //             case ProjectStatus.InProgress:
    //                 query = query.Where(p => now >= p.StartDate && now <= p.EndDate);
    //                 break;
    //             case ProjectStatus.Completed:
    //                 query = query.Where(p => now > p.EndDate);
    //                 break;
    //         }
    //     }
    //     
    //     var items = await query
    //         .OrderByDescending(c => c.EndDate) 
    //         .ToListAsync();
    //
    //     return items;
    // }
    // public async Task<List<Project>> FilterProjectsForStudentSortedByEndDateDescendingAsync(int studentId, string? title, string? classTitle, int? projectStatus)
    // {
    //     var query = _context.Projects
    //         .Include(p => p.Class)
    //         .Where(p => p.Class.ClassStudents.Any(cs => cs.StudentId == studentId))
    //         .AsQueryable();
    //
    //     if (!string.IsNullOrWhiteSpace(title))
    //     {
    //         query = query.Where(p => p.Title.Contains(title));
    //     }
    //
    //     if (!string.IsNullOrWhiteSpace(classTitle))
    //     {
    //         query = query.Where(p => p.Class.Title.Contains(classTitle));
    //     }
    //
    //     if (projectStatus.HasValue)
    //     {
    //         var now = DateTime.UtcNow;
    //
    //         switch ((ProjectStatus)projectStatus.Value)
    //         {
    //             case ProjectStatus.NotStarted:
    //                 query = query.Where(p => now < p.StartDate);
    //                 break;
    //             case ProjectStatus.InProgress:
    //                 query = query.Where(p => now >= p.StartDate && now <= p.EndDate);
    //                 break;
    //             case ProjectStatus.Completed:
    //                 query = query.Where(p => now > p.EndDate);
    //                 break;
    //         }
    //     }
    //
    //     var result = await query
    //         .OrderByDescending(p => p.EndDate)
    //         .ToListAsync();
    //
    //     return result;
    // }
    // public async Task<List<Project>> FilterProjectsForInstructorSortedByProjectTitleAscendingAsync(int instructorId, string? title, string? classTitle, int? projectStatus)
    // {
    //     var query = _context.Projects
    //         .Include(p => p.Class)
    //         .Where(c => c.Class.InstructorId == instructorId)
    //         .AsQueryable();
    //
    //     if (!string.IsNullOrWhiteSpace(title))
    //     {
    //         query = query.Where(c => c.Title.Contains(title));
    //     }
    //     if (!string.IsNullOrWhiteSpace(classTitle))
    //     {
    //         query = query.Where(c => c.Class.Title.Contains(classTitle));
    //     }
    //
    //     if (projectStatus.HasValue)
    //     {
    //         var now = DateTime.UtcNow;
    //
    //         switch ((ProjectStatus)projectStatus.Value)
    //         {
    //             case ProjectStatus.NotStarted:
    //                 query = query.Where(p => now < p.StartDate);
    //                 break;
    //             case ProjectStatus.InProgress:
    //                 query = query.Where(p => now >= p.StartDate && now <= p.EndDate);
    //                 break;
    //             case ProjectStatus.Completed:
    //                 query = query.Where(p => now > p.EndDate);
    //                 break;
    //         }
    //     }
    //     
    //     var items = await query
    //         .OrderBy(c => c.Title) 
    //         .ToListAsync();
    //
    //     return items;
    // }
    // public async Task<List<Project>> FilterProjectsForStudentSortedByByProjectTitleAscendingAsync(int studentId, string? title, string? classTitle, int? projectStatus)
    // {
    //     var query = _context.Projects
    //         .Include(p => p.Class)
    //         .Where(p => p.Class.ClassStudents.Any(cs => cs.StudentId == studentId))
    //         .AsQueryable();
    //
    //     if (!string.IsNullOrWhiteSpace(title))
    //     {
    //         query = query.Where(p => p.Title.Contains(title));
    //     }
    //
    //     if (!string.IsNullOrWhiteSpace(classTitle))
    //     {
    //         query = query.Where(p => p.Class.Title.Contains(classTitle));
    //     }
    //
    //     if (projectStatus.HasValue)
    //     {
    //         var now = DateTime.UtcNow;
    //
    //         switch ((ProjectStatus)projectStatus.Value)
    //         {
    //             case ProjectStatus.NotStarted:
    //                 query = query.Where(p => now < p.StartDate);
    //                 break;
    //             case ProjectStatus.InProgress:
    //                 query = query.Where(p => now >= p.StartDate && now <= p.EndDate);
    //                 break;
    //             case ProjectStatus.Completed:
    //                 query = query.Where(p => now > p.EndDate);
    //                 break;
    //         }
    //     }
    //
    //     var result = await query
    //         .OrderBy(p => p.Title)
    //         .ToListAsync();
    //
    //     return result;
    // }
    // public async Task<List<Project>> FilterProjectsForInstructorSortedByProjectTitleDescendingAsync(int instructorId, string? title, string? classTitle, int? projectStatus)
    // {
    //     var query = _context.Projects
    //         .Include(p => p.Class)
    //         .Where(c => c.Class.InstructorId == instructorId)
    //         .AsQueryable();
    //
    //     if (!string.IsNullOrWhiteSpace(title))
    //     {
    //         query = query.Where(c => c.Title.Contains(title));
    //     }
    //     if (!string.IsNullOrWhiteSpace(classTitle))
    //     {
    //         query = query.Where(c => c.Class.Title.Contains(classTitle));
    //     }
    //
    //     if (projectStatus.HasValue)
    //     {
    //         var now = DateTime.UtcNow;
    //
    //         switch ((ProjectStatus)projectStatus.Value)
    //         {
    //             case ProjectStatus.NotStarted:
    //                 query = query.Where(p => now < p.StartDate);
    //                 break;
    //             case ProjectStatus.InProgress:
    //                 query = query.Where(p => now >= p.StartDate && now <= p.EndDate);
    //                 break;
    //             case ProjectStatus.Completed:
    //                 query = query.Where(p => now > p.EndDate);
    //                 break;
    //         }
    //     }
    //     
    //     var items = await query
    //         .OrderByDescending(c => c.Title) 
    //         .ToListAsync();
    //
    //     return items;
    // }
    // public async Task<List<Project>> FilterProjectsForStudentSortedByProjectTitleDescendingAsync(int studentId, string? title, string? classTitle, int? projectStatus)
    // {
    //     var query = _context.Projects
    //         .Include(p => p.Class)
    //         .Where(p => p.Class.ClassStudents.Any(cs => cs.StudentId == studentId))
    //         .AsQueryable();
    //
    //     if (!string.IsNullOrWhiteSpace(title))
    //     {
    //         query = query.Where(p => p.Title.Contains(title));
    //     }
    //
    //     if (!string.IsNullOrWhiteSpace(classTitle))
    //     {
    //         query = query.Where(p => p.Class.Title.Contains(classTitle));
    //     }
    //
    //     if (projectStatus.HasValue)
    //     {
    //         var now = DateTime.UtcNow;
    //
    //         switch ((ProjectStatus)projectStatus.Value)
    //         {
    //             case ProjectStatus.NotStarted:
    //                 query = query.Where(p => now < p.StartDate);
    //                 break;
    //             case ProjectStatus.InProgress:
    //                 query = query.Where(p => now >= p.StartDate && now <= p.EndDate);
    //                 break;
    //             case ProjectStatus.Completed:
    //                 query = query.Where(p => now > p.EndDate);
    //                 break;
    //         }
    //     }
    //
    //     var result = await query
    //         .OrderByDescending(p => p.Title)
    //         .ToListAsync();
    //
    //     return result;
    // }
    // public async Task<List<Project>> FilterProjectsForInstructorSortedByClassTitleAscendingAsync(int instructorId, string? title, string? classTitle, int? projectStatus)
    // {
    //     var query = _context.Projects
    //         .Include(p => p.Class)
    //         .Where(c => c.Class.InstructorId == instructorId)
    //         .AsQueryable();
    //
    //     if (!string.IsNullOrWhiteSpace(title))
    //     {
    //         query = query.Where(c => c.Title.Contains(title));
    //     }
    //     if (!string.IsNullOrWhiteSpace(classTitle))
    //     {
    //         query = query.Where(c => c.Class.Title.Contains(classTitle));
    //     }
    //
    //     if (projectStatus.HasValue)
    //     {
    //         var now = DateTime.UtcNow;
    //
    //         switch ((ProjectStatus)projectStatus.Value)
    //         {
    //             case ProjectStatus.NotStarted:
    //                 query = query.Where(p => now < p.StartDate);
    //                 break;
    //             case ProjectStatus.InProgress:
    //                 query = query.Where(p => now >= p.StartDate && now <= p.EndDate);
    //                 break;
    //             case ProjectStatus.Completed:
    //                 query = query.Where(p => now > p.EndDate);
    //                 break;
    //         }
    //     }
    //     
    //     var items = await query
    //         .OrderBy(c => c.Class.Title) 
    //         .ToListAsync();
    //
    //     return items;
    // }
    // public async Task<List<Project>> FilterProjectsForStudentSortedByClassTitleAscendingAsync(int studentId, string? title, string? classTitle, int? projectStatus)
    // {
    //     var query = _context.Projects
    //         .Include(p => p.Class)
    //         .Where(p => p.Class.ClassStudents.Any(cs => cs.StudentId == studentId))
    //         .AsQueryable();
    //
    //     if (!string.IsNullOrWhiteSpace(title))
    //     {
    //         query = query.Where(p => p.Title.Contains(title));
    //     }
    //
    //     if (!string.IsNullOrWhiteSpace(classTitle))
    //     {
    //         query = query.Where(p => p.Class.Title.Contains(classTitle));
    //     }
    //
    //     if (projectStatus.HasValue)
    //     {
    //         var now = DateTime.UtcNow;
    //
    //         switch ((ProjectStatus)projectStatus.Value)
    //         {
    //             case ProjectStatus.NotStarted:
    //                 query = query.Where(p => now < p.StartDate);
    //                 break;
    //             case ProjectStatus.InProgress:
    //                 query = query.Where(p => now >= p.StartDate && now <= p.EndDate);
    //                 break;
    //             case ProjectStatus.Completed:
    //                 query = query.Where(p => now > p.EndDate);
    //                 break;
    //         }
    //     }
    //
    //     var result = await query
    //         .OrderBy(p => p.Class.Title)
    //         .ToListAsync();
    //
    //     return result;
    // }
    // public async Task<List<Project>> FilterProjectsForInstructorSortedByClassTitleDescendingAsync(int instructorId, string? title, string? classTitle, int? projectStatus)
    // {
    //     var query = _context.Projects
    //         .Include(p => p.Class)
    //         .Where(c => c.Class.InstructorId == instructorId)
    //         .AsQueryable();
    //
    //     if (!string.IsNullOrWhiteSpace(title))
    //     {
    //         query = query.Where(c => c.Title.Contains(title));
    //     }
    //     if (!string.IsNullOrWhiteSpace(classTitle))
    //     {
    //         query = query.Where(c => c.Class.Title.Contains(classTitle));
    //     }
    //
    //     if (projectStatus.HasValue)
    //     {
    //         var now = DateTime.UtcNow;
    //
    //         switch ((ProjectStatus)projectStatus.Value)
    //         {
    //             case ProjectStatus.NotStarted:
    //                 query = query.Where(p => now < p.StartDate);
    //                 break;
    //             case ProjectStatus.InProgress:
    //                 query = query.Where(p => now >= p.StartDate && now <= p.EndDate);
    //                 break;
    //             case ProjectStatus.Completed:
    //                 query = query.Where(p => now > p.EndDate);
    //                 break;
    //         }
    //     }
    //     
    //     var items = await query
    //         .OrderByDescending(c => c.Class.Title) 
    //         .ToListAsync();
    //
    //     return items;
    // }
    // public async Task<List<Project>> FilterProjectsForStudentSortedByClassTitleDescendingAsync(int studentId, string? title, string? classTitle, int? projectStatus)
    // {
    //     var query = _context.Projects
    //         .Include(p => p.Class)
    //         .Where(p => p.Class.ClassStudents.Any(cs => cs.StudentId == studentId))
    //         .AsQueryable();
    //
    //     if (!string.IsNullOrWhiteSpace(title))
    //     {
    //         query = query.Where(p => p.Title.Contains(title));
    //     }
    //
    //     if (!string.IsNullOrWhiteSpace(classTitle))
    //     {
    //         query = query.Where(p => p.Class.Title.Contains(classTitle));
    //     }
    //
    //     if (projectStatus.HasValue)
    //     {
    //         var now = DateTime.UtcNow;
    //
    //         switch ((ProjectStatus)projectStatus.Value)
    //         {
    //             case ProjectStatus.NotStarted:
    //                 query = query.Where(p => now < p.StartDate);
    //                 break;
    //             case ProjectStatus.InProgress:
    //                 query = query.Where(p => now >= p.StartDate && now <= p.EndDate);
    //                 break;
    //             case ProjectStatus.Completed:
    //                 query = query.Where(p => now > p.EndDate);
    //                 break;
    //         }
    //     }
    //
    //     var result = await query
    //         .OrderByDescending(p => p.Class.Title)
    //         .ToListAsync();
    //
    //     return result;
    // }
    
    public async Task<bool> IsProjectForInstructorAsync(int projectId, int instructorId)
    {
        return await _context.Projects
            .AnyAsync(p => p.Id == projectId && p.Class.InstructorId == instructorId);
    }

    public async Task<bool> IsProjectForStudentAsync(int projectId, int studentId)
    {
        return await _context.ClassStudents
            .AnyAsync(cs =>
                cs.Class.Projects.Any(p => p.Id == projectId) &&
                cs.StudentId == studentId);
    }

    public async Task<bool> ProjectExists(int projectId)
    {
        return await _context.Projects.AnyAsync(p => p.Id == projectId);
    }
    public async Task<bool> IsProjectNameDuplicateAsync(int classId, string projectName)
    {
        return await _context.Projects
            .AnyAsync(p => p.ClassId == classId && p.Title == projectName);
    }
    public async Task<List<Project>> GetProjectsByClassIdAsync(int classId)
    {
        return await _context.Projects
            .Where(p => p.ClassId == classId)
            .OrderByDescending(p => p.EndDate)                
            .ToListAsync();
    }

}