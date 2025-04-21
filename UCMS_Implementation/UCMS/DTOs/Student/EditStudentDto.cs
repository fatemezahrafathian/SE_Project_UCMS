using System.ComponentModel.DataAnnotations;
using UCMS.Models;

namespace UCMS.DTOs.Student
{
    public class EditStudentDto
    {
        [MaxLength(50)]
        public string? StudentNumber { get; set; }
        public string? Major { get; set; }
        public int? EnrollmentYear { get; set; }
        public int? University { get; set; }
        public int? EducationLevel { get; set; }
    }
}
