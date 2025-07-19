using BankingApplication.BsonSerializers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace BankingApplication.Configurations
{
    public static class BsonSerializers
    {
        public static IConfiguration ConfigureBsonSerializers(this IConfiguration configuration)
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            BsonSerializer.RegisterSerializer(new DateOnlyBsonSerializer());
            BsonSerializer.RegisterSerializer(new DateTimeBsonSerializer());

            return configuration;
        }
    }
}