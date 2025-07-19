using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;

namespace BankingApplication.Configurations
{
    public static class ConventionRegistries
    {
        public static IConfiguration ConfigureConventionRegistries(this IConfiguration configuration)
        {
            var conventionPack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new EnumRepresentationConvention(BsonType.String)
            };

            ConventionRegistry.Register("banking", conventionPack, _ => true);

            return configuration;
        }
    }
}