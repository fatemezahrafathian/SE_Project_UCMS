using UCMS.DTOs;
using UCMS.DTOs.ProjectDto;
using UCMS.Models;

namespace UCMS.Services.ProjectService;

public interface IProjectService
{
    Task<ServiceResponse<GetProjectForInstructorDto>> CreateProjectAsync(int classId,CreateProjectDto dto);
    Task<ServiceResponse<GetProjectForInstructorDto>> UpdateProjectAsync(int classId, int projectId, PatchProjectDto dto);
}