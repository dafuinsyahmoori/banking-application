using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingApplication.JsonConverters
{
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

            return new DateOnly(year, month, day);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteNumber("year", value.Year);
            writer.WriteNumber("month", value.Month);
            writer.WriteNumber("day", value.Day);

            writer.WriteEndObject();
        }
    }
}