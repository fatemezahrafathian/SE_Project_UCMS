using Microsoft.AspNetCore.Mvc;
using UCMS.DTOs.Enum;
using UCMS.Models;


namespace UCMS.Controllers
{
    [Route("api/enums")]
    [ApiController]
    public class EnumApiController : ControllerBase
    {

        [HttpGet("university")]
        public IActionResult GetUniversities()
        {
            var universities = Enum.GetValues(typeof(University))
                        .Cast<University>()
                        .Select(u => new UniversityDto
                        {
                            Id = (int)u,
                            Name = u.ToString()
                        })
                        .ToList();
            return Ok(universities);
        }

        [HttpGet("educationLevel")]
        public IActionResult GetEducationLevels()
        {
            var educationLevels
                = Enum.GetValues(typeof(EducationLevel))
                        .Cast<EducationLevel>()
                        .Select(e => new EducationLevelDto
                        {
                            Id = (int)e,
                            Name = e.ToString()
                        })
                        .ToList();
            return Ok(educationLevels);
        }

        [HttpGet("instructorRank")]
        public IActionResult GetInstructorRanks()
        {
            var instructorRanks = Enum.GetValues(typeof(InstructorRank))
                        .Cast<InstructorRank>()
                        .Select(i => new InstructorRankDto
                        {
                            Id = (int)i,
                            Name = i.ToString()
                        })
                        .ToList();
            return Ok(instructorRanks);
        }

        [HttpGet("gender")]
        public IActionResult GetGenders()
        {
            var genders = Enum.GetValues(typeof(Gender))
                        .Cast<Gender>()
                        .Select(g => new GenderDto
                        {
                            Id = (int)g,
                            Name = g.ToString()
                        })
                        .ToList();
            return Ok(genders);
        }

    }
}
