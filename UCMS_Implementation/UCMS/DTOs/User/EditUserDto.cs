using UCMS.Models;

namespace UCMS.DTOs.User
{
    public class EditUserDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Gender? Gender { get; set; }
        public string? Address { get; set; }
        public string? Bio { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
