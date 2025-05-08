using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingApplication.JsonConverters
{
    public class DateTimeJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();
            reader.Read();

            var year = reader.GetInt32();

            reader.Read();
            reader.Read();

            var month = reader.GetInt32();

            reader.Read();
            reader.Read();

            var day = reader.GetInt32();

            reader.Read();
            reader.Read();

            var hour = reader.GetInt32();

            reader.Read();
            reader.Read();

            var minute = reader.GetInt32();

            reader.Read();
            reader.Read();

            var second = reader.GetInt32();

            reader.Read();

            return new DateTime(year, month, day, hour, minute, second);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(value.Month);

            writer.WriteStartObject();

            writer.WriteNumber("year", value.Year);
            writer.WriteString("month", monthName);
            writer.WriteNumber("day", value.Day);
            writer.WriteNumber("hour", value.Hour);
            writer.WriteNumber("minute", value.Minute);
            writer.WriteNumber("second", value.Second);

            writer.WriteEndObject();
        }
    }
}