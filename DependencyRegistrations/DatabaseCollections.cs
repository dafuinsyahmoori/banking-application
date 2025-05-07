using BankingApplication.Entities;
using MongoDB.Driver;

namespace BankingApplication.DependencyRegistrations
{
    public static class DatabaseCollections
    {
        public static IServiceCollection AddDatabaseCollections(this IServiceCollection services)
        {
            return services
                .AddSingleton(serviceProvider =>
                {
                    var database = serviceProvider.GetRequiredService<IMongoDatabase>();
                    return database.GetCollection<User>("users");
                })
                .AddSingleton(serviceProvider =>
                {
                    var database = serviceProvider.GetRequiredService<IMongoDatabase>();
                    return database.GetCollection<Account>("accounts");
                });
        }
    }
}