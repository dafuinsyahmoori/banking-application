using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace BankingApplication.BsonSerializers
{
    public class TimeSpanBsonSerializer : IBsonSerializer<TimeSpan>
    {
        public Type ValueType => typeof(TimeSpan);

        public TimeSpan Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            if (context.Reader.CurrentBsonType is BsonType.Document)
            {
                context.Reader.ReadStartDocument();

                var hours = context.Reader.ReadInt32();
                var minutes = context.Reader.ReadInt32();
                var seconds = context.Reader.ReadInt32();

                context.Reader.ReadEndDocument();

                return new TimeSpan(hours, minutes, seconds);
            }

            throw new NotSupportedException("Cannot convert to TimeSpan.");
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TimeSpan value)
        {
            context.Writer.WriteStartDocument();

            context.Writer.WriteName("hours");
            context.Writer.WriteInt32(value.Hours);

            context.Writer.WriteName("minutes");
            context.Writer.WriteInt32(value.Minutes);

            context.Writer.WriteName("seconds");
            context.Writer.WriteInt32(value.Seconds);

            context.Writer.WriteEndDocument();
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            var timeSpan = (TimeSpan)value;

            context.Writer.WriteStartDocument();

            context.Writer.WriteName("hours");
            context.Writer.WriteInt32(timeSpan.Hours);

            context.Writer.WriteName("minutes");
            context.Writer.WriteInt32(timeSpan.Minutes);

            context.Writer.WriteName("seconds");
            context.Writer.WriteInt32(timeSpan.Seconds);

            context.Writer.WriteEndDocument();
        }

        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return Deserialize(context, args);
        }
    }
}