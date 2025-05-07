using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace BankingApplication.BsonSerializers
{
    public class DateOnlyBsonSerializer : IBsonSerializer<DateOnly>
    {
        public Type ValueType => typeof(DateOnly);

        public DateOnly Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var type = context.Reader.CurrentBsonType;

            if (type is BsonType.Document)
            {
                context.Reader.ReadStartDocument();

                var year = context.Reader.ReadInt32();
                var month = context.Reader.ReadInt32();
                var day = context.Reader.ReadInt32();

                context.Reader.ReadEndDocument();

                return new DateOnly(year, month, day);
            }

            throw new NotSupportedException("Cannot convert to DateOnly.");
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateOnly value)
        {
            context.Writer.WriteStartDocument();

            context.Writer.WriteName("year");
            context.Writer.WriteInt32(value.Year);

            context.Writer.WriteName("month");
            context.Writer.WriteInt32(value.Month);

            context.Writer.WriteName("day");
            context.Writer.WriteInt32(value.Day);

            context.Writer.WriteEndDocument();
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            var date = (DateOnly)value;

            context.Writer.WriteStartDocument();

            context.Writer.WriteName("year");
            context.Writer.WriteInt32(date.Year);

            context.Writer.WriteName("month");
            context.Writer.WriteInt32(date.Month);

            context.Writer.WriteName("day");
            context.Writer.WriteInt32(date.Day);

            context.Writer.WriteEndDocument();
        }

        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return Deserialize(context, args);
        }
    }
}