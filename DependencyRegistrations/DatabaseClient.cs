using MongoDB.Driver;

namespace BankingApplication.DependencyRegistrations
{
    public static class DatabaseClient
    {
        public static IServiceCollection AddDatabaseClient(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddSingleton<IMongoClient>(
                new MongoClient(
                    configuration.GetConnectionString("BankingApplication")
                )
            );
        }
    }
}