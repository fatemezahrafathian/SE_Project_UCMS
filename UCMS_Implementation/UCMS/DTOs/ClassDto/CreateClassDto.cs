using System.ComponentModel.DataAnnotations;
using UCMS.Models;

namespace UCMS.DTOs.ClassDto;

public class CreateClassDto
{
    [Required, MaxLength(100)]
    public string Title { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
    [Required]
    public ClassIdentifierType IdentifierType { get; set; }
    [Required, MinLength(1, ErrorMessage = "At least one schedule is required.")]
    public List<ClassScheduleDto> Schedules { get; set; }
}