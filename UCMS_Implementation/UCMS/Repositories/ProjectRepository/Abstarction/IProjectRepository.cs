using UCMS.DTOs.ProjectDto;
using UCMS.Models;

namespace UCMS.Repositories.ProjectRepository.Abstarction;

public interface IProjectRepository
{
    Task AddAsync(Project project);
    Task<Project?> GetProjectByIdAsync(int projectId);
    Task<Project?> GetSimpleProjectByIdAsync(int projectId);
    Task UpdateAsync(Project project);
    Task DeleteAsync(Project project);

    Task<List<Project>> FilterProjectsForInstructorAsync(int instructorId, string? title, string? classTitle,
        int? projectStatus, string orderBy, bool descending);

    Task<List<Project>> FilterProjectsForStudentAsync(int instructorId, string? title, string? classTitle,
        int? projectStatus, string orderBy, bool descending);
    Task<bool> IsProjectForInstructorAsync(int projectId, int instructorId);
    Task<bool> IsProjectForStudentAsync(int projectId, int studentId);
    Task<bool> ProjectExists(int projectId);
    Task<bool> IsProjectNameDuplicateAsync(int classId, string projectName);
    Task<List<Project>> GetProjectsByClassIdAsync(int classId);

}