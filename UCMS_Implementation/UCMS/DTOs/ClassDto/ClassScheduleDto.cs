using System.Text.Json.Serialization;

namespace UCMS.DTOs.ClassDto;
public class ClassScheduleDto
{
    public DayOfWeek DayOfWeek { get; set; }
    // [JsonConverter(typeof(TimeSpanConverter))]
    public TimeSpan StartTime { get; set; }

    // [JsonConverter(typeof(TimeSpanConverter))]
    public TimeSpan EndTime { get; set; }
    
}