using System.ComponentModel.DataAnnotations;

namespace UCMS.Models;

public class StudentExam
{
    public int Id { get; set; }

    [Required]
    public int StudentId { get; set; }
    public Student Student { get; set; }

    [Required]
    public int ExamId { get; set; }
    public Exam Exam { get; set; }

    public double? Score { get; set; }
    
}