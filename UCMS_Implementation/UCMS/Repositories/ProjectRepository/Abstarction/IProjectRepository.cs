using UCMS.Models;

namespace UCMS.Repositories.ProjectRepository.Abstarction;

public interface IProjectRepository
{
    Task AddAsync(Project project);
    Task<Project?> GetProjectByIdAsync(int projectId);
    Task UpdateAsync(Project project);
    Task DeleteAsync(Project project);
}