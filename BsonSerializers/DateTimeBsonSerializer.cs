using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace BankingApplication.BsonSerializers
{
    public class DateTimeBsonSerializer : IBsonSerializer<DateTime>
    {
        public Type ValueType => typeof(DateTime);

        public DateTime Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            if (context.Reader.CurrentBsonType is BsonType.Document)
            {
                context.Reader.ReadStartDocument();

                var year = context.Reader.ReadInt32();
                var month = context.Reader.ReadInt32();
                var day = context.Reader.ReadInt32();
                var hour = context.Reader.ReadInt32();
                var minute = context.Reader.ReadInt32();
                var second = context.Reader.ReadInt32();

                context.Reader.ReadEndDocument();

                return new DateTime(year, month, day, hour, minute, second);
            }

            throw new NotSupportedException("Cannot convert to DateOnly.");
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTime value)
        {
            context.Writer.WriteStartDocument();

            context.Writer.WriteName("year");
            context.Writer.WriteInt32(value.Year);

            context.Writer.WriteName("month");
            context.Writer.WriteInt32(value.Month);

            context.Writer.WriteName("day");
            context.Writer.WriteInt32(value.Day);

            context.Writer.WriteName("hour");
            context.Writer.WriteInt32(value.Hour);

            context.Writer.WriteName("minute");
            context.Writer.WriteInt32(value.Minute);

            context.Writer.WriteName("second");
            context.Writer.WriteInt32(value.Second);

            context.Writer.WriteEndDocument();
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            var dateTime = (DateTime)value;

            context.Writer.WriteStartDocument();

            context.Writer.WriteName("year");
            context.Writer.WriteInt32(dateTime.Year);

            context.Writer.WriteName("month");
            context.Writer.WriteInt32(dateTime.Month);

            context.Writer.WriteName("day");
            context.Writer.WriteInt32(dateTime.Day);

            context.Writer.WriteName("hour");
            context.Writer.WriteInt32(dateTime.Hour);

            context.Writer.WriteName("minute");
            context.Writer.WriteInt32(dateTime.Minute);

            context.Writer.WriteName("second");
            context.Writer.WriteInt32(dateTime.Second);

            context.Writer.WriteEndDocument();
        }

        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return Deserialize(context, args);
        }
    }
}