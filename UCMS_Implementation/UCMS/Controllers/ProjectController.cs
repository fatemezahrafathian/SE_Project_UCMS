using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using UCMS.Attributes;
using UCMS.Data;
using UCMS.DTOs.ClassDto;
using UCMS.DTOs.ProjectDto;
using UCMS.Services.ClassService.Abstraction;
using UCMS.Services.ProjectService;

namespace UCMS.Controllers;
[Route("api/projects")]
[ApiController]
public class ProjectController: ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly DataContext _context;

    public ProjectController(IProjectService projectService, DataContext context)
    {
        _projectService = projectService;
        _context = context;
    }
    [RoleBasedAuthorization("Instructor")]
    [HttpPost("")]
    public async Task<IActionResult> CreateProject(int classId,[FromForm] CreateProjectDto dto)
    {
        var response = await _projectService.CreateProjectAsync(classId,dto);

        if (response.Success)
        {
            /////////////////////////////////////////////
            return Ok(response.Message);
        }

        return BadRequest(new {message = response.Message});
    }
}