using UCMS.Models;

namespace UCMS.DTOs.Student
{
    public class GetStudentDto
    {
        public string? StudentNumber { get; set; }
        public string? Major { get; set; }
        public int? EnrollmentYear { get; set; }
        public string? University { get; set; }
        public string? EducationLevel { get; set; }
    }
}
