using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UCMS.Models;

public class ClassSchedule
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ClassId { get; set; }

    // [ForeignKey("ClassId")]
    // public Class Class { get; set; }

    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }
}