using UCMS.Data;
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
}