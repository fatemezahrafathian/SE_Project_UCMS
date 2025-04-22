using System.Text.Json;
using System.Text.Json.Serialization;

namespace UCMS.DTOs.ClassDto
{
    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(@"hh\:mm\:ss"));
        }

        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var timeString = reader.GetString();

            if (TimeSpan.TryParse(timeString, out var time))
            {
                return time;
            }

            throw new JsonException($"Invalid time format: {timeString}");
        }
    }
}