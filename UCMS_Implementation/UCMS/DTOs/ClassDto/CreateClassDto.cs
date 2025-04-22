using System.ComponentModel.DataAnnotations;
using UCMS.Models;

namespace UCMS.DTOs.ClassDto;

public class CreateClassDto
{
    [Required, MaxLength(100)]
    public string Title { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
    public IFormFile? ProfileImage { get; set; }

    // [Required, MinLength(1, ErrorMessage = "At least one schedule is required.")]
    public List<ClassScheduleDto> Schedules { get; set; } = new List<ClassScheduleDto>();}