using System.Text.Json;
using System.Text.Json.Serialization;
using BankingApplication.Entities.Enums;

namespace BankingApplication.JsonConverters
{
    public class TransactionTypeJsonConverter : JsonConverter<TransactionType>
    {
        public override TransactionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, TransactionType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("F"));
        }
    }
}