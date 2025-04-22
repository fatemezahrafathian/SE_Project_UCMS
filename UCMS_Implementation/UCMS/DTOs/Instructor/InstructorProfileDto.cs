using UCMS.DTOs.RoleDto;
using UCMS.Models;

namespace UCMS.DTOs.Instructor
{
    public class InstructorProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public string? Bio { get; set; }
        public GetRoleDto? Role { get; set; }
        public int? Rank { get; set; }
        public string? Department { get; set; }
        public int? University { get; set; }
    }
}
