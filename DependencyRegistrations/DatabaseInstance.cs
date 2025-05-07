using MongoDB.Driver;

namespace BankingApplication.DependencyRegistrations
{
    public static class DatabaseInstance
    {
        public static IServiceCollection AddDatabaseInstance(this IServiceCollection services)
        {
            return services.AddSingleton(serviceProvider =>
            {
                var client = serviceProvider.GetRequiredService<IMongoClient>();
                return client.GetDatabase("banking");
            });
        }
    }
}