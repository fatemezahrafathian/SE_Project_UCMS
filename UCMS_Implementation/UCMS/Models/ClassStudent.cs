using System.ComponentModel.DataAnnotations;

namespace UCMS.Models;

public class ClassStudent
{
    [Key]
    public int ClassId { get; set; }
    [Key]
    public int StudentId { get; set; }
    [Required]
    public DateTime JoinedAt { get; set; }
}