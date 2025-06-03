using UCMS.DTOs;
using UCMS.DTOs.ProjectDto;
using UCMS.Models;

namespace UCMS.Services.ProjectService;

public interface IProjectService
{
    Task<ServiceResponse<GetProjectForInstructorDto>> CreateProjectAsync(int classId,CreateProjectDto dto);
    Task<ServiceResponse<GetProjectForInstructorDto>> UpdateProjectAsync(int classId, int projectId, PatchProjectDto dto);
    Task<ServiceResponse<string>> DeleteProjectAsync(int projectId);
    Task<ServiceResponse<GetProjectForInstructorDto>> GetProjectByIdForInstructorAsync(int projectId);
    Task<ServiceResponse<GetProjectForStudentDto>> GetProjectByIdForStudentAsync(int projectId);
    Task<ServiceResponse<FileDownloadDto>> HandleDownloadProjectFileAsync(int projectId);
    Task<ServiceResponse<List<GetProjectListForInstructorDto>>> GetProjectsForInstructor(FilterProjectsForInstructorDto dto);
    Task<ServiceResponse<List<GetProjectListForStudentDto>>> GetProjectsForStudent(FilterProjectsForStudentDto dto);

}