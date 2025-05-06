using UCMS.Models;

namespace UCMS.Repositories.ProjectRepository.Abstarction;

public interface IProjectRepository
{
    Task AddAsync(Project project);
}