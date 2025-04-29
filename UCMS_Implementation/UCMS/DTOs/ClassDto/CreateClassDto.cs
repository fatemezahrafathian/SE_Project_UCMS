namespace UCMS.DTOs.ClassDto;

public class CreateClassDto
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public IFormFile? ProfileImage { get; set; }
    public List<ClassScheduleDto> Schedules { get; set; } = new();
}

// public class CreateClassDto
// {
//     // [Required, MaxLength(100)]
//     public string Title { get; set; }
//
//     // [MaxLength(500)]
//     public string? Description { get; set; }
//     
//     // [Required, MinLength(6), MaxLength(100)]
//     public string Password { get; set; }
//
//     // [Required, Compare("Password", ErrorMessage = "Passwords do not match.")]
//     public string ConfirmPassword { get; set; }
//
//     public DateOnly? StartDate { get; set; }
//
//     public DateOnly? EndDate { get; set; }
//     public IFormFile? ProfileImage { get; set; }
//
//     // [Required, MinLength(1, ErrorMessage = "At least one schedule is required.")]
//     public List<ClassScheduleDto> Schedules { get; set; } = new List<ClassScheduleDto>();}