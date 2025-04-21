using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UCMS.Models;

public class Student
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }

    [MaxLength(50)]
    public string? StudentNumber { get; set; }

    [MaxLength(100)]
    public string? Major { get; set; }

    public EducationLevel? EducationLevel { get; set; }

    public int? EnrollmentYear { get; set; }

}
