using System.ComponentModel.DataAnnotations;
using UCMS.DTOs.RoleDto;
using UCMS.Models;

namespace UCMS.DTOs.User
{
    public class OutputUserDto
    {
        public string? Email { get; set; }

        public string? Username { get; set; }

        public string? University { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Gender { get; set; }

        public GetRoleDto? Role { get; set; }

        public string? Address { get; set; }

        public string? Bio { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? ProfileImagePath { get; set; }
    }
}