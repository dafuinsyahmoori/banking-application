using System.Text.Json;
using System.Text.Json.Serialization;
using BankingApplication.Entities.Enums;

namespace BankingApplication.JsonConverters
{
    public class WithdrawalStatusJsonConverter : JsonConverter<WithdrawalStatus>
    {
        public override WithdrawalStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, WithdrawalStatus value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("F"));
        }
    }
}