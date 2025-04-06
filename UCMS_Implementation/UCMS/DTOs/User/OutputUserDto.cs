using System.ComponentModel.DataAnnotations;

namespace UCMS.DTOs.User
{
    public class OutputUserDto
    {
        public string? Email { get; set; }

        public string? UserName { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Address { get; set; }

        public string? Bio { get; set; }

        public string? ProfileImagePath { get; set; }
    }
}