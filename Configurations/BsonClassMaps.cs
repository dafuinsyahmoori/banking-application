using BankingApplication.Entities;
using MongoDB.Bson.Serialization;

namespace BankingApplication.Configurations
{
    public static class BsonClassMaps
    {
        public static IConfiguration ConfigureBsonClassMaps(this IConfiguration configuration)
        {
            BsonClassMap.RegisterClassMap<User>(classMap =>
            {
                classMap.AutoMap();

                classMap.MapMember(u => u.MiddleName).SetIgnoreIfNull(true);
                classMap.MapMember(u => u.LastName).SetIgnoreIfNull(true);
            });

            BsonClassMap.RegisterClassMap<TransactionHistory>(classMap =>
            {
                classMap.AutoMap();

                classMap.MapMember(th => th.ReceiverAccountNumber).SetIgnoreIfNull(true);
                classMap.MapMember(th => th.SenderAccountNumber).SetIgnoreIfNull(true);
            });

            return configuration;
        }
    }
}