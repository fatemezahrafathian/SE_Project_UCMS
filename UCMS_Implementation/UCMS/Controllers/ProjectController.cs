using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.DTOs.ProjectDto;
using UCMS.Services.ProjectService;

namespace UCMS.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ProjectController: ControllerBase
{
    private readonly IProjectService _projectService;
    
    public ProjectController(IProjectService projectService)
    {
        _projectService = projectService;
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpPost("")]
    public async Task<IActionResult> CreateProject(int classId,[FromForm] CreateProjectDto dto)
    {
        var response = await _projectService.CreateProjectAsync(classId,dto);

        if (response.Success)
        {
            return CreatedAtAction(nameof(GetProjectForInstructor), new {projectId = response.Data.Id}, response);
        }

        return BadRequest(new {message = response.Message});
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpPatch("{projectId}")]
    public async Task<IActionResult> UpdateProject(int projectId, [FromForm] PatchProjectDto dto)
    {
        var response = await _projectService.UpdateProjectAsync(projectId, dto);
        if (!response.Success)
            return NotFound(response.Message);

        return Ok(new {Data=response.Data,message = response.Message});
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpDelete("{projectId}")]
    public async Task<IActionResult> DeleteProject(int projectId)
    {
        var response = await _projectService.DeleteProjectAsync(projectId);
        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Message);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("Instructor/{projectId}")]
    public async Task<IActionResult> GetProjectForInstructor(int projectId)
    {
        var response = await _projectService.GetProjectByIdForInstructorAsync(projectId);
        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Student")]
    [HttpGet("Student/{projectId}")]
    public async Task<IActionResult> GetProjectForStudent(int projectId)
    {
        var response = await _projectService.GetProjectByIdForStudentAsync(projectId);
        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Student")]
    [HttpGet("{projectId}/downloadForStudent")]
    public async Task<IActionResult> DownloadProjectFileForStudent(int projectId)
    {
        var response = await _projectService.HandleDownloadProjectFileAsync(projectId);
        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("{projectId}/downloadForInstructor")]
    public async Task<IActionResult> DownloadProjectFileForInstructor(int projectId)
    {
        var response = await _projectService.HandleDownloadProjectFileAsync(projectId);
        if (!response.Success)
            return NotFound(response.Message);

        return File(response.Data.FileBytes, response.Data.ContentType, response.Data.FileName);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("instructor")]
    public async Task<IActionResult> GetProjectsForInstructor([FromQuery] FilterProjectsForInstructorDto dto)
    {
        var response = await _projectService.GetProjectsForInstructor(dto);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Student")]
    [HttpGet("Student")]
    public async Task<IActionResult> GetProjectsForStudent([FromQuery] FilterProjectsForStudentDto dto)
    {
        var response = await _projectService.GetProjectsForStudent(dto);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpGet("{classId}/projectsOfClass/Instructor")]
    public async Task<IActionResult> GetProjectsOfClassForInstructor(int classId)
    {
        var response = await _projectService.GetProjectsOfClassForInstructorAsync(classId);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }
    [RoleBasedAuthorization("Student")]
    [HttpGet("{classId}/projectsOfClass/Student")]
    public async Task<IActionResult> GetProjectsOfClassForStudent(int classId)
    {
        var response = await _projectService.GetProjectsOfClassForStudentAsync(classId);

        if (!response.Success)
            return NotFound(response.Message);

        return Ok(response.Data);
    }

}