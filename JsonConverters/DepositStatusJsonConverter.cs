using System.Text.Json;
using System.Text.Json.Serialization;
using BankingApplication.Entities.Enums;

namespace BankingApplication.JsonConverters
{
    public class DepositStatusJsonConverter : JsonConverter<DepositStatus>
    {
        public override DepositStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, DepositStatus value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("F"));
        }
    }
}