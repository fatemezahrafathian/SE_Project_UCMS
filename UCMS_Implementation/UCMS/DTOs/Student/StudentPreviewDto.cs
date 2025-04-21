using UCMS.Models;

namespace UCMS.DTOs.Student
{
    public class StudentPreviewDto
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Major { get; set; }

        public EducationLevel? EducationLevel { get; set; }
        public string? ProfileImagePath { get; set; }

    }
}
