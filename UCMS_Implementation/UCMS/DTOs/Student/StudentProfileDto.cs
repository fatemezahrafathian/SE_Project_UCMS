using UCMS.DTOs.RoleDto;
using UCMS.Models;

namespace UCMS.DTOs.Student
{
    public class StudentProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public string? Bio { get; set; }
        public GetRoleDto? Role { get; set; }
        public EducationLevel? EducationLevel { get; set; }
        public string? Major {  get; set; }
        public int? EnrollmentYear { get; set; }

    }
}
