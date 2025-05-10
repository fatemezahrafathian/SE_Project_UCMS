using Microsoft.AspNetCore.Mvc;
using UCMS.Data;
using UCMS.Services.ClassService.Abstraction;

namespace UCMS.Controllers;
[Route("api/[controller]")]
[ApiController]
public class PhaseController:ControllerBase
{
    private readonly DataContext _context;

    public PhaseController(IClassService classService, DataContext context)
    {
        _context = context;
    }
    
}