using UCMS.DTOs;
using UCMS.DTOs.ProjectDto;
using UCMS.Models;

namespace UCMS.Services.ProjectService;

public interface IProjectService
{
    Task<ServiceResponse<GetProjectForInstructorDto>> CreateProjectAsync(int classId,CreateProjectDto dto);
}